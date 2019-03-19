using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using EtsyAccess.Exceptions;
using EtsyAccess.Misc;
using EtsyAccess.Models;
using EtsyAccess.Services.Authentication;
using Newtonsoft.Json;
using Polly;

namespace EtsyAccess.Services
{
	public class BaseService
	{
		protected const string BaseUrl = "https://openapi.etsy.com";
		protected const int RetryCount = 5;
		private const string ShopsInfoUrl = "/v2/shops/{0}";
		protected readonly int? shopId;

		protected readonly HttpClient httpClient;
		protected readonly OAuthenticator authenticator;
		private Func< string > _additionalLogInfo;

		/// <summary>
		///	Extra logging information
		/// </summary>
		public Func<string> AdditionalLogInfo
		{
			get { return this._additionalLogInfo ?? ( () => string.Empty ); }
			set => _additionalLogInfo = value;
		}

		public BaseService( string consumerKey, string consumerSecret ) : this( consumerKey, consumerSecret, null,
			null, null )
		{
		}

		public BaseService( string consumerKey, string consumerSecret, string token, string tokenSecret, int? shopId )
		{
			this.shopId = shopId;

			httpClient = new HttpClient()
			{
				BaseAddress = new Uri( BaseUrl )
			};

			authenticator = new OAuthenticator( consumerKey, consumerSecret, token, tokenSecret);
		}

		/// <summary>
		///	Returns shop info
		/// </summary>
		/// <param name="shopName">Etsy's shop name</param>
		/// <returns></returns>
		public async Task< Shop > GetShopInfo( string shopName )
		{
			var mark = Mark.CreateNew();
			IEnumerable< Shop > response = null;
			string url = String.Format( ShopsInfoUrl, shopName );

			try
			{
				EtsyLogger.LogStarted( this.CreateMethodCallInfo( url, mark, additionalInfo : this.AdditionalLogInfo() ) );

				response = await GetEntitiesAsync< Shop >( url ).ConfigureAwait( false );

				EtsyLogger.LogEnd( this.CreateMethodCallInfo( url, mark, methodResult: response.ToJson(), additionalInfo : this.AdditionalLogInfo() ) );
			}
			catch ( Exception exception )
			{
				var etsyException = new EtsyException( this.CreateMethodCallInfo( url, mark, additionalInfo : this.AdditionalLogInfo() ), exception );
				EtsyLogger.LogTraceException( etsyException );
				throw etsyException;
			}

			return response.FirstOrDefault();
		}

		/// <summary>
		///	Returns entities asynchronously
		/// </summary>
		/// <typeparam name="T">Entities that should be received from service endpoint</typeparam>
		/// <param name="url">Relative url to endpoint</param>
		/// <param name="result"></param>
		/// <param name="mark">Method tracing mark</param>
		/// <returns></returns>
		public async Task< IEnumerable< T > > GetEntitiesAsync< T >( string url, List< T > result = null, Mark mark = null )
		{
			if( mark.IsBlank() )
				mark = Mark.CreateNew();

			url = authenticator.GetUriWithOAuthQueryParameters( BaseUrl + url );

			try
			{
				var responseContent = await Policy.HandleResult< string >( entityRaw => entityRaw == null )
					.WaitAndRetryAsync( RetryCount,
						retryAttempt => TimeSpan.FromSeconds( Math.Pow( 2, retryAttempt ) ),
						( entityRaw, timeSpan, retryCount, context ) =>
						{
							EtsyLogger.LogTraceRetryStarted($"Request failed. Waiting { timeSpan } before next attempt. Retry attempt { retryCount }");
						})
					.ExecuteAsync( async () =>
					{
						string entityRaw = null;

						try
						{
							var httpResponse = await httpClient.GetAsync( url ).ConfigureAwait( false );
							string content = await httpResponse.Content.ReadAsStringAsync()
								.ConfigureAwait( false );

							// etsy errors
							HandleEtsyEndpointErrorResponse( content );

							// rate limits
							LogRateLimits( httpResponse );

							if ( httpResponse.StatusCode != HttpStatusCode.OK )
								throw new EtsyException(content);

							entityRaw = content;
						}
						catch ( Exception exception )
						{
							if ( exception is EtsyInvalidSignatureException )
								// regenerating OAuth header
								url = authenticator.GetUriWithOAuthQueryParameters( url );

							var etsyException = new EtsyException( this.CreateMethodCallInfo( url, mark, additionalInfo : this.AdditionalLogInfo() ), exception );
							EtsyLogger.LogTraceException( etsyException );
						}

						return entityRaw;
					});

				var response = JsonConvert.DeserializeObject< EtsyResponse< T > >( responseContent );

				if (result == null)
					result = new List<T>();

				result.AddRange( response.Results );

				// handle pagination
				if ( response.Pagination.NextPage != null 
				     && response.Pagination.NextOffset != null )
				{
					int offset = response.Pagination.NextOffset.Value;

					url = url.Replace( BaseUrl, "" );

					if ( url.Contains('?') )
						url += "&";
					else
						url += "?";

					url += url.Contains( "offset" ) ? url.Replace( $"offset={offset - 1}", $"offset={offset}" ) : $"offset={offset}";

					await GetEntitiesAsync( url, result ).ConfigureAwait( false );
				}
			}
			catch ( Exception exception )
			{
				var etsyException = new EtsyException( this.CreateMethodCallInfo( url, mark, additionalInfo : this.AdditionalLogInfo() ), exception );
				EtsyLogger.LogTraceException( etsyException );
				throw etsyException;
			}

			return result;
		}

		/// <summary>
		///	Returns entity asynchronously
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="url"></param>
		///  <param name="mark"></param>
		/// <returns></returns>
		public async Task< T > GetEntityAsync< T >( string url, Mark mark = null )
		{
			if( mark.IsBlank() )
				mark = Mark.CreateNew();

			url = authenticator.GetUriWithOAuthQueryParameters( BaseUrl + url );

			try
			{
				var responseContent = await Policy.HandleResult< string >( entityRaw => entityRaw == null )
					.WaitAndRetryAsync( RetryCount,
						retryAttempt => TimeSpan.FromSeconds( Math.Pow( 2, retryAttempt ) ),
						( entityRaw, timeSpan, retryCount, context ) =>
						{
							EtsyLogger.LogTraceRetryStarted($"Request failed. Waiting { timeSpan } before next attempt. Retry attempt { retryCount }");
						})
					.ExecuteAsync( async () =>
					{
						string entityRaw = null;

						try
						{
							var httpResponse = await httpClient.GetAsync( url ).ConfigureAwait( false );
							string content = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait( false );

							// handle Etsy error
							HandleEtsyEndpointErrorResponse( content );

							LogRateLimits( httpResponse );

							if ( httpResponse.StatusCode != HttpStatusCode.OK )
								throw new EtsyException(content);

							entityRaw = content;
						}
						catch ( Exception exception )
						{
							if ( exception is EtsyInvalidSignatureException )
								// regenerating OAuth header
								url = authenticator.GetUriWithOAuthQueryParameters( url );

							var etsyException = new EtsyException( this.CreateMethodCallInfo( url, mark, additionalInfo : this.AdditionalLogInfo() ), exception );
							EtsyLogger.LogTraceException( etsyException );
						}

						return entityRaw;
					});

				var response = JsonConvert.DeserializeObject< EtsyResponseSingleEntity< T > >( responseContent );

				return response.Result;
			}
			catch ( Exception exception )
			{
				var etsyException = new EtsyException( this.CreateMethodCallInfo( url, mark, additionalInfo : this.AdditionalLogInfo() ), exception );
				EtsyLogger.LogTraceException( etsyException );
				throw etsyException;
			}
		}

		/// <summary>
		///	Updates data
		/// </summary>
		/// <param name="payload"></param>
		/// <param name="url"></param>
		/// <param name="mark"></param>
		/// <returns></returns>
		public async Task PutAsync( string url, Dictionary<string, string> payload, Mark mark = null )
		{
			if( mark.IsBlank() )
				mark = Mark.CreateNew();

			try
			{
				await Policy.Handle<EtsyException>()
					.WaitAndRetryAsync( RetryCount,
						retryAttempt => TimeSpan.FromSeconds( Math.Pow( 2, retryAttempt ) ),
						( entityRaw, timeSpan, retryCount, context ) =>
						{
							EtsyLogger.LogTraceRetryStarted($"Request failed. Waiting { timeSpan } before next attempt. Retry attempt { retryCount }");
						})
					.ExecuteAsync( async () =>
					{
						try
						{
							var content = new FormUrlEncodedContent( payload );

							if ( !url.Contains(BaseUrl) )
								url = BaseUrl + url;

							url = authenticator.GetUriWithOAuthQueryParameters( url, "PUT", payload );

							var response = await httpClient.PutAsync( url, content ).ConfigureAwait( false );
							string responseStr = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

							// handle server response maybe some error happened
							HandleEtsyEndpointErrorResponse( responseStr );

							LogRateLimits(  response );

							if ( response.StatusCode != HttpStatusCode.OK )
								throw new EtsyException( responseStr );
						}
						catch ( Exception exception )
						{
							var etsyException = new EtsyException( this.CreateMethodCallInfo( url, mark, additionalInfo : this.AdditionalLogInfo() ), exception );
							EtsyLogger.LogTraceException( etsyException );

							throw etsyException;
						}
					});
			}
			catch (Exception exception)
			{
				var etsyException = new EtsyException( this.CreateMethodCallInfo( url, mark, additionalInfo : this.AdditionalLogInfo() ), exception );
				EtsyLogger.LogTraceException( etsyException );
				throw etsyException;
			}
		}

		/// <summary>
		///	Handles Etsy server error responses
		/// </summary>
		/// <param name="response"></param>
		protected void HandleEtsyEndpointErrorResponse( string response )
		{
			if ( string.IsNullOrEmpty(response) )
				return;

			if ( response.IndexOf("signature_invalid", StringComparison.InvariantCulture ) > -1)
				throw new EtsyInvalidSignatureException( response );
		}

		/// <summary>
		///	Creates method calling detailed information
		/// </summary>
		/// <param name="url">Absolute path to service endpoint</param>
		/// <param name="mark">Unique stamp to track concrete method</param>
		/// <param name="errors">Errors</param>
		/// <param name="methodResult">Service endpoint raw result</param>
		/// <param name="additionalInfo">Extra logging information</param>
		/// <param name="memberName">Method name</param>
		/// <returns></returns>
		protected string CreateMethodCallInfo( string url = "", Mark mark = null, string errors = "", string methodResult = "", string additionalInfo = "", [ CallerMemberName ] string memberName = "" )
		{
			string serviceEndPoint = null;
			string requestParameters = null;

			if ( !string.IsNullOrEmpty( url ) )
			{
				Uri uri = new Uri( url.Contains( BaseUrl ) ? url : BaseUrl + url );

				serviceEndPoint = uri.LocalPath;
				requestParameters = uri.Query;
			}

			var str = string.Format(
				"{{MethodName: {0}, Mark: '{1}', ServiceEndPoint: {2}, {3} {4}{5}}}",
				memberName,
				mark ?? Mark.Blank(),
				string.IsNullOrWhiteSpace(serviceEndPoint) ? string.Empty : serviceEndPoint,
				string.IsNullOrWhiteSpace(requestParameters) ? string.Empty : ", RequestParameters: " + requestParameters,
				string.IsNullOrWhiteSpace( errors ) ? string.Empty : ", Errors:" + errors,
				string.IsNullOrWhiteSpace( methodResult ) ? string.Empty : ", Result:" + methodResult,
				string.IsNullOrWhiteSpace( additionalInfo ) ? string.Empty : ", " + additionalInfo
			);
			return str;
		}

		/// <summary>
		///	Logs API limits
		/// </summary>
		/// <param name="response">HTTP message</param>
		private void LogRateLimits( HttpResponseMessage response )
		{
			var rateLimit = response.Headers.GetValues("X-RateLimit-Limit" )
				.FirstOrDefault();
			var rateLimitRemaining = response.Headers.GetValues("X-RateLimit-Remaining" )
				.FirstOrDefault();

			if ( rateLimit != null
				&& rateLimitRemaining != null )
				EtsyLogger.LogTrace($"Rate limit { rateLimit }, rate limit remaining { rateLimitRemaining }" );
		}
	}
}
