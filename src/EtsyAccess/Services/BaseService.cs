using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
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
		private readonly string ApplicationKey;
		private readonly string SharedSecret;
		protected readonly EtsyConfig Config;
		protected readonly HttpClient HttpClient;
		protected readonly OAuthenticator Authenticator;
		private Func< string > _additionalLogInfo;
		private readonly Throttler Throttler;

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
				BaseAddress = new Uri( Config.ApiBaseUrl ),
				Timeout = TimeSpan.FromMilliseconds( config.RequestTimeoutMs )
			};

			SetSslSettings();

			Authenticator = new OAuthenticator( ApplicationKey, SharedSecret, Config.Token, Config.TokenSecret );
		}

		/// <summary>
		///	Returns entities asynchronously
		/// </summary>
		/// <typeparam name="T">Entities that should be received from service endpoint</typeparam>
		/// <param name="url">Url to endpoint</param>
		/// <param name="cancellationToken">Cancellation token for cancelling call to endpoint</param>
		/// <param name="mark">Method tracing mark</param>
		/// <returns></returns>
		protected async Task< IEnumerable< T > > GetEntitiesAsync< T >( string url, CancellationToken cancellationToken, Mark mark = null )
		{
			var result = new List< T >();
			var response = await GetEntitiesAsyncByOffset< T >( url, cancellationToken, 0, mark ).ConfigureAwait( false );

			result.AddRange( response.Results );

			// handle pagination
			while ( response.Pagination.NextOffset != null )
			{
				var nextOffset = response.Pagination.NextOffset.Value;
				response = await GetEntitiesAsyncByOffset< T >( url, cancellationToken, nextOffset, mark ).ConfigureAwait( false );
				result.AddRange( response.Results );
			}

			return result;
		}

		private async Task< EtsyResponse< T > > GetEntitiesAsyncByOffset< T >( string url, CancellationToken cancellationToken, int offset = 0, Mark mark = null)
		{
			if ( offset > 0 )
			{
				url += $"&offset={offset}";
			}

			if ( cancellationToken.IsCancellationRequested )
			{
				var exceptionDetails = CreateMethodCallInfo( url, mark, additionalInfo: this.AdditionalLogInfo() );
				throw new EtsyException( $"{exceptionDetails}. Task was cancelled" );
			}

			var responseContent = await Throttler.ExecuteAsync(() =>
			{
				return new ActionPolicy( Config.RetryAttempts )
					.ExecuteAsync(async () =>
						{
							if ( !url.Contains( Config.ApiBaseUrl ) )
								url = Config.ApiBaseUrl + url;

							url = Authenticator.GetUriWithOAuthQueryParameters( url );

							var httpResponse = await HttpClient.GetAsync( url, cancellationToken ).ConfigureAwait( false );
							var content = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait( false );

							LogRateLimits( httpResponse, CreateMethodCallInfo( url, mark, additionalInfo: this.AdditionalLogInfo() ) );

							ThrowIfError( httpResponse, content );

							return content;
						}, 
						( timeSpan, retryCount ) =>
						{
							var retryDetails = CreateMethodCallInfo( url, mark, additionalInfo: this.AdditionalLogInfo() );
							EtsyLogger.LogTraceRetryStarted( timeSpan.Seconds, retryCount, retryDetails );
						},
						() => CreateMethodCallInfo( url, mark, additionalInfo: this.AdditionalLogInfo() ),
						cancellationToken );
			}).ConfigureAwait( false );

			return JsonConvert.DeserializeObject< EtsyResponse< T > >( responseContent );
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
			if ( cancellationToken.IsCancellationRequested )
			{
				var exceptionDetails = CreateMethodCallInfo( url, mark, additionalInfo: this.AdditionalLogInfo() );
				throw new EtsyException( $"{exceptionDetails}. Task was cancelled" );
			}

			var responseContent = await Throttler.ExecuteAsync(() =>
			{
				return new ActionPolicy( Config.RetryAttempts )
					.ExecuteAsync(async () =>
						{
							if ( !url.Contains( Config.ApiBaseUrl ) )
								url = Config.ApiBaseUrl + url;

							url = Authenticator.GetUriWithOAuthQueryParameters( url );

							var httpResponse = await HttpClient.GetAsync( url, cancellationToken ).ConfigureAwait( false );
							var content = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait( false );

							LogRateLimits( httpResponse, CreateMethodCallInfo( url, mark, additionalInfo: this.AdditionalLogInfo() )  );

							ThrowIfError( httpResponse, content );

							return content;
						}, 
						( timeSpan, retryCount ) =>
						{
							var retryDetails = CreateMethodCallInfo( url, mark, additionalInfo: this.AdditionalLogInfo() );
							EtsyLogger.LogTraceRetryStarted( timeSpan.Seconds, retryCount, retryDetails );
						},
						() => CreateMethodCallInfo( url, mark, additionalInfo: this.AdditionalLogInfo() ),
						cancellationToken );
			}).ConfigureAwait( false );

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
		protected Task PutAsync( string url, Dictionary< string, string > payload, CancellationToken token, Mark mark = null )
		{
			if ( token.IsCancellationRequested )
			{
				var exceptionDetails = CreateMethodCallInfo( url, mark, additionalInfo: this.AdditionalLogInfo() );
				throw new EtsyException( $"{exceptionDetails}. Task was cancelled" );
			}

			return Throttler.ExecuteAsync(() =>
			{
				return new ActionPolicy( Config.RetryAttempts )
					.ExecuteAsync(async () =>
						{
							var stringPayload = JsonConvert.SerializeObject( payload );
							var content = new StringContent( stringPayload, Encoding.UTF8, "application/json" );

							if ( !url.Contains( Config.ApiBaseUrl ) )
								url = Config.ApiBaseUrl + url;

							url = Authenticator.GetUriWithOAuthQueryParameters( url, "PUT", payload );

							var response = await HttpClient.PutAsync( url, content, token ).ConfigureAwait( false );
							var responseStr = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
						
							LogRateLimits( response, CreateMethodCallInfo( url, mark, additionalInfo: this.AdditionalLogInfo() ) );

							ThrowIfError( response, responseStr );

							return responseStr;
						}, 
						( timeSpan, retryCount ) =>
						{
							var retryDetails = CreateMethodCallInfo( url, mark, additionalInfo: this.AdditionalLogInfo() );
							EtsyLogger.LogTraceRetryStarted( timeSpan.Seconds, retryCount, retryDetails );
						},
						() => CreateMethodCallInfo( url, mark, additionalInfo: this.AdditionalLogInfo() ),
						token );
			});
		}

		/// <summary>
		///	Post data
		/// </summary>
		/// <param name="url"></param>
		/// <param name="payload"></param>
		/// <param name="token"></param>
		/// <param name="mark"></param>
		/// <returns></returns>
		protected Task PostAsync( string url, Dictionary< string, string > payload, CancellationToken token, Mark mark = null )
		{
			if ( token.IsCancellationRequested )
			{
				var exceptionDetails = CreateMethodCallInfo( url, mark, additionalInfo: this.AdditionalLogInfo() );
				throw new EtsyException( $"{exceptionDetails}. Task was cancelled" );
			}

			return Throttler.ExecuteAsync(() =>
			{
				return new ActionPolicy( Config.RetryAttempts )
					.ExecuteAsync(async () =>
						{
							var content = new FormUrlEncodedContent( payload );

							if ( !url.Contains( Config.ApiBaseUrl ) )
								url = Config.ApiBaseUrl + url;

							url = Authenticator.GetUriWithOAuthQueryParameters( url, "POST", payload );

							var response = await HttpClient.PostAsync( url, content, token ).ConfigureAwait( false );
							var responseStr = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
						
							LogRateLimits( response, CreateMethodCallInfo( url, mark, additionalInfo: this.AdditionalLogInfo() ) );

							ThrowIfError( response, responseStr );

							return responseStr;
						}, 
						( timeSpan, retryCount ) =>
						{
							var retryDetails = CreateMethodCallInfo( url, mark, additionalInfo: this.AdditionalLogInfo() );
							EtsyLogger.LogTraceRetryStarted( timeSpan.Seconds, retryCount, retryDetails );
						},
						() => CreateMethodCallInfo( url, mark, additionalInfo: this.AdditionalLogInfo() ),
						token );
			});
		}

		/// <summary>
		///	Handles Etsy server error responses
		/// </summary>
		/// <param name="response">Http response</param>
		/// <param name="message">response message</param>
		public void ThrowIfError( HttpResponseMessage response, string message )
		{
			var responseStatusCode = response.StatusCode;
			if ( response.IsSuccessStatusCode )
				return;

			if ( message.IndexOf("signature_invalid", StringComparison.InvariantCulture ) > -1 )
				throw new EtsyInvalidSignatureException( message );

			if ( message.IndexOf("exceeded your quota", StringComparison.InvariantCulture ) > -1 )
				throw new EtsyApiLimitsExceeded( GetEtsyLimits( response ), message );

			if ( responseStatusCode == HttpStatusCode.BadGateway )
				throw new EtsyBadGatewayException( message );

			if ( responseStatusCode == HttpStatusCode.Conflict )
				throw new EtsyConflictException( message );

			throw new EtsyServerException( message, (int)responseStatusCode );
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
		/// <param name="requestPayload">Payload for PUT calls and errors</param>
		/// <returns></returns>
		public string CreateMethodCallInfo( string url = "", Mark mark = null, string errors = "", string methodResult = "", string additionalInfo = "", [ CallerMemberName ] string memberName = "", string requestPayload = "" )
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
				"{{MethodName: {0}, Mark: '{1}', ServiceEndPoint: '{2}', {3} {4}{5}{6}{7}}}",
				memberName,
				mark ?? Mark.Blank(),
				string.IsNullOrWhiteSpace( serviceEndPoint ) ? string.Empty : serviceEndPoint,
				string.IsNullOrWhiteSpace( requestParameters ) ? string.Empty : ", RequestParameters: " + requestParameters,
				string.IsNullOrWhiteSpace( errors ) ? string.Empty : ", Errors:" + errors,
				string.IsNullOrWhiteSpace( methodResult ) ? string.Empty : ", Result:" + methodResult,
				string.IsNullOrWhiteSpace( requestPayload ) ? string.Empty : ", RequestPayload: " + requestPayload,
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
			{
				Throttler.DayLimit = limits.TotalAvailableRequests;
				Throttler.DayLimitRemaining = limits.CallsRemaining;

				EtsyLogger.LogTrace( String.Format( "{0}, Total calls: {1}, Remaining calls: {2} ", info, limits.TotalAvailableRequests, limits.CallsRemaining ));
			}
		}

		/// <summary>
		///	Extracts API limits from server response
		/// </summary>
		/// <param name="response"></param>
		/// <returns></returns>
		public EtsyLimits GetEtsyLimits( HttpResponseMessage response )
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
