using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CuttingEdge.Conditions;
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
		// max total time for attempts 15 seconds (14)
		protected const int RetryCount = 3;
		private const string ShopsInfoUrl = "/v2/shops/{0}";
		protected readonly int? ShopId;

		protected readonly HttpClient HttpClient;
		protected readonly OAuthenticator Authenticator;
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
			this.ShopId = shopId;

			HttpClient = new HttpClient()
			{
				BaseAddress = new Uri( BaseUrl )
			};

			Authenticator = new OAuthenticator( consumerKey, consumerSecret, token, tokenSecret);
		}

		/// <summary>
		///	Returns shop info
		/// </summary>
		/// <param name="shopName">Etsy's shop name</param>
		/// <returns></returns>
		public async Task< Shop > GetShopInfo( string shopName )
		{
			Condition.Requires( shopName ).IsNotNullOrEmpty();

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
		protected async Task< IEnumerable< T > > GetEntitiesAsync< T >( string url, List< T > result = null, Mark mark = null )
		{
			var responseContent = await Policy.Handle< EtsyNetworkException >()
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
						if ( !url.Contains( BaseUrl ) )
							url = BaseUrl + url;

						url = Authenticator.GetUriWithOAuthQueryParameters( url );

						var httpResponse = await HttpClient.GetAsync( url ).ConfigureAwait( false );
						string content = await httpResponse.Content.ReadAsStringAsync()
							.ConfigureAwait( false );

						// rate limits
						LogRateLimits( httpResponse );

						// etsy errors
						ThrowIfError( httpResponse, content );

						entityRaw = content;
					}
					catch ( Exception exception )
					{
						EtsyException etsyException = null;
						string exceptionDetails = this.CreateMethodCallInfo( url, mark, additionalInfo: this.AdditionalLogInfo() );

						if ( exception is EtsyServerException )
							etsyException = new EtsyException(exceptionDetails, exception );
						else
							etsyException = new EtsyNetworkException(exceptionDetails, exception);

						EtsyLogger.LogTraceException( etsyException );

						throw etsyException;
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

			return result;
		}

		/// <summary>
		///	Returns entity asynchronously
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="url"></param>
		///  <param name="mark"></param>
		/// <returns></returns>
		protected async Task< T > GetEntityAsync< T >( string url, Mark mark = null )
		{
			var responseContent = await Policy.Handle< EtsyNetworkException >()
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
						if ( !url.Contains( BaseUrl ) )
							url = BaseUrl + url;

						url = Authenticator.GetUriWithOAuthQueryParameters( url );

						var httpResponse = await HttpClient.GetAsync( url ).ConfigureAwait( false );
						var content = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait( false );

						LogRateLimits( httpResponse );

						ThrowIfError( httpResponse, content );

						entityRaw = content;
					}
					catch ( Exception exception )
					{
						EtsyException etsyException = null;
						string exceptionDetails = this.CreateMethodCallInfo( url, mark, additionalInfo: this.AdditionalLogInfo() );
						
						if ( exception is EtsyServerException )
							etsyException = new EtsyException( exceptionDetails, exception );
						else
							etsyException = new EtsyNetworkException( exceptionDetails, exception );

						EtsyLogger.LogTraceException( etsyException );

						throw etsyException;
					}

					return entityRaw;
				});

			var response = JsonConvert.DeserializeObject< EtsyResponseSingleEntity< T > >( responseContent );

			return response.Result;
		}

		/// <summary>
		///	Updates data
		/// </summary>
		/// <param name="payload"></param>
		/// <param name="url"></param>
		/// <param name="mark"></param>
		/// <returns></returns>
		protected async Task PutAsync( string url, Dictionary<string, string> payload, Mark mark = null )
		{
			await Policy.Handle< EtsyNetworkException >()
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

						url = Authenticator.GetUriWithOAuthQueryParameters( url, "PUT", payload );

						var response = await HttpClient.PutAsync( url, content ).ConfigureAwait( false );
						var responseStr = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

						LogRateLimits(  response );

						// handle server response maybe some error happened
						ThrowIfError( response, responseStr );
					}
					catch ( Exception exception )
					{
						EtsyException etsyException = null;
						string exceptionDetails = this.CreateMethodCallInfo( url, mark, additionalInfo: this.AdditionalLogInfo() );

						if ( exception is EtsyServerException )
							etsyException = new EtsyException(exceptionDetails, exception );
						else
							etsyException = new EtsyNetworkException(exceptionDetails, exception);

						EtsyLogger.LogTraceException( etsyException );

						throw etsyException;
					}
				});
		}

		/// <summary>
		///	Handles Etsy server error responses
		/// </summary>
		/// <param name="response">Http response</param>
		/// <param name="message">response message</param>
		protected void ThrowIfError( HttpResponseMessage response, string message )
		{
			HttpStatusCode responseStatusCode = response.StatusCode;

			if ( responseStatusCode == HttpStatusCode.OK
					|| responseStatusCode == HttpStatusCode.Created )
				return;

			if ( message.IndexOf("signature_invalid", StringComparison.InvariantCulture ) > -1)
				throw new EtsyInvalidSignatureException( message );

			var errorDetailHeaderValue = response.Headers.GetValues("X-Error-Detail").FirstOrDefault();

			if ( errorDetailHeaderValue != null )
				message += String.Format(", details: {0}", errorDetailHeaderValue);

			throw new EtsyServerException( message, (int)responseStatusCode);
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
