using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CuttingEdge.Conditions;
using EtsyAccess.Exceptions;
using EtsyAccess.Shared;
using EtsyAccess.Models;
using EtsyAccess.Models.Configuration;
using EtsyAccess.Models.Throttling;
using EtsyAccess.Services.Authentication;
using Newtonsoft.Json;

namespace EtsyAccess.Services
{
	public class BaseService
	{
		protected readonly string ApplicationKey;
		protected readonly string SharedSecret;
		protected readonly EtsyConfig Config;
		protected readonly HttpClient HttpClient;
		protected readonly OAuthenticator Authenticator;
		private Func< string > _additionalLogInfo;
		private CancellationTokenSource _requestTimeoutCancellationTokenSource;
		public readonly Throttler Throttler;

		/// <summary>
		///	Extra logging information
		/// </summary>
		public Func<string> AdditionalLogInfo
		{
			get { return this._additionalLogInfo ?? ( () => string.Empty ); }
			set => _additionalLogInfo = value;
		}

		public BaseService( string applicationKey, string sharedSecret, EtsyConfig config, Throttler throttler )
		{
			Condition.Requires( applicationKey ).IsNotNullOrEmpty();
			Condition.Requires( sharedSecret ).IsNotNullOrEmpty();
			Condition.Requires( config ).IsNotNull();
			Condition.Requires( throttler ).IsNotNull();

			this.ApplicationKey = applicationKey;
			this.SharedSecret = sharedSecret;
			this.Config = config;
			this.Throttler = throttler;

			HttpClient = new HttpClient()
			{
				BaseAddress = new Uri( Config.ApiBaseUrl ) 
			};

			SetSslSettings();

			Authenticator = new OAuthenticator( ApplicationKey, SharedSecret, Config.Token, Config.TokenSecret );

			_requestTimeoutCancellationTokenSource = new CancellationTokenSource();
		}

		/// <summary>
		///	Returns entities asynchronously
		/// </summary>
		/// <typeparam name="T">Entities that should be received from service endpoint</typeparam>
		/// <param name="url">Url to endpoint</param>
		/// <param name="result"></param>
		/// <param name="cancellationToken">Cancellation token for cancelling call to endpoint</param>
		/// <param name="mark">Method tracing mark</param>
		/// <returns></returns>
		protected async Task< IEnumerable< T > > GetEntitiesAsync< T >( string url, CancellationToken cancellationToken, List< T > result = null, Mark mark = null )
		{
			var responseContent = await Throttler.ExecuteAsync(() =>
			{
				return new ActionPolicy( Config.RetryAttempts )
					.ExecuteAsync(async () =>
						{
							if ( !url.Contains( Config.ApiBaseUrl ) )
								url = Config.ApiBaseUrl + url;

							url = Authenticator.GetUriWithOAuthQueryParameters( url );

							_requestTimeoutCancellationTokenSource.CancelAfter( Config.RequestTimeoutMs );
							var linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource( cancellationToken, _requestTimeoutCancellationTokenSource.Token );

							var httpResponse = await HttpClient.GetAsync( url, linkedCancellationTokenSource.Token ).ConfigureAwait( false );
							string content = await httpResponse.Content.ReadAsStringAsync()
								.ConfigureAwait( false );

							LogRateLimits( httpResponse, CreateMethodCallInfo( url, mark, additionalInfo: this.AdditionalLogInfo() )  );

							ThrowIfError( httpResponse, content );

							return content;
						}, 
						(timeSpan, retryCount) =>
						{
							string retryDetails = CreateMethodCallInfo( url, mark, additionalInfo: this.AdditionalLogInfo() );
							EtsyLogger.LogTraceRetryStarted( timeSpan.Seconds, retryCount, retryDetails );

							_requestTimeoutCancellationTokenSource.CancelAfter( Config.RequestTimeoutMs * (int) Math.Pow( 2, retryCount ) );
						},
						() => CreateMethodCallInfo( url, mark, additionalInfo: this.AdditionalLogInfo() ),
						EtsyLogger.LogTraceException);
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

				await GetEntitiesAsync( url, cancellationToken, result ).ConfigureAwait( false );
			}

			return result;
		}

		/// <summary>
		///	Returns entity asynchronously
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="url"></param>
		/// <param name="cancellationToken"></param>
		/// <param name="mark"></param>
		/// <returns></returns>
		protected async Task< T > GetEntityAsync< T >( string url, CancellationToken cancellationToken, Mark mark = null )
		{
			var responseContent = await Throttler.ExecuteAsync(() =>
			{
				return new ActionPolicy( Config.RetryAttempts )
					.ExecuteAsync(async () =>
						{
							if ( !url.Contains( Config.ApiBaseUrl ) )
								url = Config.ApiBaseUrl + url;

							url = Authenticator.GetUriWithOAuthQueryParameters( url );

							_requestTimeoutCancellationTokenSource.CancelAfter( Config.RequestTimeoutMs );
							var linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource( cancellationToken, _requestTimeoutCancellationTokenSource.Token );

							var httpResponse = await HttpClient.GetAsync( url, linkedCancellationTokenSource.Token ).ConfigureAwait( false );
							var content = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait( false );

							LogRateLimits( httpResponse, CreateMethodCallInfo( url, mark, additionalInfo: this.AdditionalLogInfo() )  );

							ThrowIfError( httpResponse, content );

							return content;
						}, 
						(timeSpan, retryCount) =>
						{
							string retryDetails = CreateMethodCallInfo( url, mark, additionalInfo: this.AdditionalLogInfo() );
							EtsyLogger.LogTraceRetryStarted( timeSpan.Seconds, retryCount, retryDetails );

							_requestTimeoutCancellationTokenSource.CancelAfter( Config.RequestTimeoutMs * (int) Math.Pow( 2, retryCount ) );
						},
						() => CreateMethodCallInfo( url, mark, additionalInfo: this.AdditionalLogInfo() ),
						EtsyLogger.LogTraceException);
			});

			var response = JsonConvert.DeserializeObject< EtsyResponseSingleEntity< T > >( responseContent );

			return response.Result;
		}

		/// <summary>
		///	Updates data
		/// </summary>
		/// <param name="payload"></param>
		/// <param name="url"></param>
		/// <param name="token"></param>
		/// <param name="mark"></param>
		/// <returns></returns>
		protected Task PutAsync(string url, Dictionary<string, string> payload, CancellationToken token, Mark mark = null)
		{
			return Throttler.ExecuteAsync(() =>
			{
				return new ActionPolicy( Config.RetryAttempts )
					.ExecuteAsync(async () =>
						{
							var content = new FormUrlEncodedContent( payload );

							if ( !url.Contains( Config.ApiBaseUrl ) )
								url = Config.ApiBaseUrl + url;

							url = Authenticator.GetUriWithOAuthQueryParameters( url, "PUT", payload );

							_requestTimeoutCancellationTokenSource.CancelAfter( Config.RequestTimeoutMs );
							var linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource( token, _requestTimeoutCancellationTokenSource.Token );

							var response = await HttpClient.PutAsync( url, content, linkedCancellationTokenSource.Token ).ConfigureAwait( false );
							var responseStr = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
						
							LogRateLimits( response, CreateMethodCallInfo( url, mark, additionalInfo: this.AdditionalLogInfo() ) );

							ThrowIfError( response, responseStr );

							return responseStr;
						}, 
						(timeSpan, retryCount) =>
						{
							string retryDetails = CreateMethodCallInfo( url, mark, additionalInfo: this.AdditionalLogInfo() );
							EtsyLogger.LogTraceRetryStarted( timeSpan.Seconds, retryCount, retryDetails );

							_requestTimeoutCancellationTokenSource.CancelAfter( Config.RequestTimeoutMs * (int) Math.Pow( 2, retryCount ) );
						},
						() => CreateMethodCallInfo( url, mark, additionalInfo: this.AdditionalLogInfo() ),
						EtsyLogger.LogTraceException);
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

			if ( message.IndexOf("signature_invalid", StringComparison.InvariantCulture ) > -1 )
				throw new EtsyInvalidSignatureException( message );

			if ( message.IndexOf("exceeded your quota", StringComparison.InvariantCulture ) > -1 )
				throw new EtsyApiLimitsExceeded( GetEtsyLimits( response ), message );

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

			IEnumerable< string > rateLimit = null;
			IEnumerable< string > rateLimitRemaining = null;

			response.Headers.TryGetValues("X-RateLimit-Limit", out rateLimit );
			response.Headers.TryGetValues("X-RateLimit-Remaining", out rateLimitRemaining );

			if ( ( rateLimit != null )
				   && (rateLimitRemaining != null ) )
				limits = new EtsyLimits( int.Parse( rateLimit.First() ), int.Parse( rateLimitRemaining.First() ) );

			return limits;
		}

		private void SetSslSettings()
		{
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
		}
	}
}
