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
using EtsyAccess.Shared;
using EtsyAccess.Models;
using EtsyAccess.Models.Configuration;
using EtsyAccess.Models.Throttling;
using EtsyAccess.Services.Authentication;
using Newtonsoft.Json;
using Polly;

namespace EtsyAccess.Services
{
	public class BaseService
	{
		protected readonly EtsyConfig Config;
		protected readonly HttpClient HttpClient;
		protected readonly OAuthenticator Authenticator;
		private Func< string > _additionalLogInfo;

		private const string ShopsInfoUrl = "/v2/shops/{0}";

		/// <summary>
		///	Extra logging information
		/// </summary>
		public Func<string> AdditionalLogInfo
		{
			get { return this._additionalLogInfo ?? ( () => string.Empty ); }
			set => _additionalLogInfo = value;
		}

		public BaseService( EtsyConfig config )
		{
			Condition.Requires( config ).IsNotNull();

			this.Config = config;

			HttpClient = new HttpClient()
			{
				BaseAddress = new Uri( Config.ApiBaseUrl )
			};

			Authenticator = new OAuthenticator( Config.ApplicationKey, Config.SharedSecret, Config.Token, Config.TokenSecret );
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
		/// <param name="url">Url to endpoint</param>
		/// <param name="result"></param>
		/// <param name="mark">Method tracing mark</param>
		/// <returns></returns>
		protected async Task< IEnumerable< T > > GetEntitiesAsync< T >( string url, List< T > result = null, Mark mark = null )
		{
			var responseContent = await Policy.Handle< EtsyNetworkException >()
				.WaitAndRetryAsync( Config.RetryAttempts,
					retryAttempt => TimeSpan.FromSeconds( Math.Pow( 2, retryAttempt ) ),
					( entityRaw, timeSpan, retryCount, context ) =>
					{
						string retryDetails = CreateMethodCallInfo( url, mark, additionalInfo: this.AdditionalLogInfo() );
						EtsyLogger.LogTraceRetryStarted( timeSpan.Seconds, retryCount, retryDetails );
					})
				.ExecuteAsync( async () =>
				{
					string entityRaw = null;

					try
					{
						if ( !url.Contains( Config.ApiBaseUrl ) )
							url = Config.ApiBaseUrl + url;

						url = Authenticator.GetUriWithOAuthQueryParameters( url );

						var httpResponse = await HttpClient.GetAsync( url ).ConfigureAwait( false );
						string content = await httpResponse.Content.ReadAsStringAsync()
							.ConfigureAwait( false );

						LogRateLimits( httpResponse, CreateMethodCallInfo( url, mark, additionalInfo: this.AdditionalLogInfo() )  );

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
			if ( ( response.Pagination.NextPage != null )
			     && ( response.Pagination.NextOffset != null ) )
			{
				int nextOffset = response.Pagination.NextPage.Value;
				int currentOffset = response.Pagination.EffectiveOffset;

				// remove current offset if exists
				url = url.Replace( String.Format("&offset={0}", currentOffset), "" );
				url += String.Format( "&offset={0}", nextOffset );

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
				.WaitAndRetryAsync( Config.RetryAttempts,
					retryAttempt => TimeSpan.FromSeconds( Math.Pow( 2, retryAttempt ) ),
					( entityRaw, timeSpan, retryCount, context ) =>
					{
						string retryDetails = CreateMethodCallInfo( url, mark, additionalInfo: this.AdditionalLogInfo() );
						EtsyLogger.LogTraceRetryStarted( timeSpan.Seconds, retryCount, retryDetails );
					})
				.ExecuteAsync( async () =>
				{
					string entityRaw = null;

					try
					{
						if ( !url.Contains( Config.ApiBaseUrl ) )
							url = Config.ApiBaseUrl + url;

						url = Authenticator.GetUriWithOAuthQueryParameters( url );

						var httpResponse = await HttpClient.GetAsync( url ).ConfigureAwait( false );
						var content = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait( false );

						LogRateLimits( httpResponse, CreateMethodCallInfo( url, mark, additionalInfo: this.AdditionalLogInfo() )  );

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
				.WaitAndRetryAsync( Config.RetryAttempts,
					retryAttempt => TimeSpan.FromSeconds( Math.Pow( 2, retryAttempt ) ),
					( entityRaw, timeSpan, retryCount, context ) =>
					{
						string retryDetails = CreateMethodCallInfo( url, mark, additionalInfo: this.AdditionalLogInfo() );
						EtsyLogger.LogTraceRetryStarted( timeSpan.Seconds, retryCount, retryDetails );
					})
				.ExecuteAsync( async () =>
				{
					try
					{
						var content = new FormUrlEncodedContent( payload );

						if ( !url.Contains( Config.ApiBaseUrl ) )
							url = Config.ApiBaseUrl + url;

						url = Authenticator.GetUriWithOAuthQueryParameters( url, "PUT", payload );

						var response = await HttpClient.PutAsync( url, content ).ConfigureAwait( false );
						var responseStr = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
						
						LogRateLimits( response, CreateMethodCallInfo( url, mark, additionalInfo: this.AdditionalLogInfo() ) );

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
				Uri uri = new Uri( url.Contains( Config.ApiBaseUrl ) ? url : Config.ApiBaseUrl + url );

				serviceEndPoint = uri.LocalPath;
				requestParameters = uri.Query;
			}

			var str = string.Format(
				"{{MethodName: {0}, Mark: '{1}', ServiceEndPoint: '{2}', {3} {4}{5}{6}}}",
				memberName,
				mark ?? Mark.Blank(),
				string.IsNullOrWhiteSpace( serviceEndPoint ) ? string.Empty : serviceEndPoint,
				string.IsNullOrWhiteSpace( requestParameters ) ? string.Empty : ", RequestParameters: " + requestParameters,
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
		/// <param name="info">extra information</param>
		private void LogRateLimits( HttpResponseMessage response, string info )
		{
			var limits = GetEtsyLimits( response );

			if ( limits != null )
				EtsyLogger.LogTrace( String.Format( "{0}, Total calls: {1}, Remaining calls: {2} ", info, limits.TotalAvailableRequests, limits.CallsRemaining ));
		}

		/// <summary>
		///	Extracts API limits from server response
		/// </summary>
		/// <param name="response"></param>
		/// <returns></returns>
		private EtsyLimits GetEtsyLimits( HttpResponseMessage response )
		{
			EtsyLimits limits = null;

			var rateLimit = response.Headers.GetValues("X-RateLimit-Limit" )
				.FirstOrDefault();
			var rateLimitRemaining = response.Headers.GetValues("X-RateLimit-Remaining" )
				.FirstOrDefault();

			if ( ( rateLimit != null )
				   && (rateLimitRemaining != null ) )
				limits = new EtsyLimits( int.Parse( rateLimit ), int.Parse( rateLimitRemaining ) );

			return limits;
		}
	}
}
