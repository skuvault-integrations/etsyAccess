using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using CuttingEdge.Conditions;
using EtsyAccess.Exceptions;
using EtsyAccess.Misc;
using NLog;
using NLog.Fluent;
using NLog.LayoutRenderers.Wrappers;
using Polly;

namespace EtsyAccess.Services.Authentication
{
	public class OAuthCredentials
	{
		public string LoginUrl { get; private set; }
		public string Token { get; private set; }
		public string TokenSecret { get; private set; }

		public OAuthCredentials(string loginUrl, string token, string tokenSecret)
		{
			LoginUrl = loginUrl;
			Token = token;
			TokenSecret = tokenSecret;
		}
	}

	/// <summary>
	///	This service is oriented on working with OAuth 1.0 credentials.
	/// You can easily get permanent credentials having only consumer key and secret.
	/// </summary>
	public class AuthenticationService : BaseService, IAuthenticationService
	{
		private const string RequestTokenUrl = "/v2/oauth/request_token";
		private const string AccessTokenUrl = "/v2/oauth/access_token";

		public AuthenticationService( string applicationKey, string sharedSecret ) : base( applicationKey, sharedSecret )
		{
		}

		/// <summary>
		///	Returns access token for making authorized API calls
		/// </summary>
		/// <returns></returns>
		public async Task< OAuthCredentials > GetPermanentCredentials( string temporaryToken, string temporaryTokenSecret, string verifierCode )
		{
			Condition.Requires( temporaryToken ).IsNotNullOrEmpty();
			Condition.Requires( temporaryTokenSecret ).IsNotNullOrEmpty();
			Condition.Requires( verifierCode ).IsNotNullOrEmpty();

			var requestParameters = new Dictionary<string, string>
			{
				{ "oauth_token", temporaryToken },
				{ "oauth_verifier", verifierCode }
			};

			return await Policy.HandleResult<OAuthCredentials>(credentials => credentials == null)
				.WaitAndRetryAsync(RetryCount,
					retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
					(entityRaw, timeSpan, retryCount, context) =>
					{
						EtsyLogger.LogTraceRetryStarted(
							$"Request failed. Waiting {timeSpan} before next attempt. Retry attempt {retryCount}");
					})
				.ExecuteAsync(async () =>
				{
					var mark = Mark.CreateNew();
					OAuthCredentials credentials = null;
					string url = BaseUrl + AccessTokenUrl;

					try
					{
						var oauthParameters = authenticator.GetOAuthRequestParameters( url, "GET", temporaryTokenSecret, requestParameters );
						url = authenticator.GetUrl( url, oauthParameters );

						EtsyLogger.LogStarted( this.CreateMethodCallInfo( url, mark, additionalInfo : this.AdditionalLogInfo() ) );

						HttpResponseMessage response = await httpClient.GetAsync( url ).ConfigureAwait( false );
						var result = response.Content.ReadAsStringAsync().Result;

						HandleEtsyEndpointErrorResponse( result );

						EtsyLogger.LogEnd( this.CreateMethodCallInfo( url, mark, methodResult: result.ToJson(), additionalInfo : this.AdditionalLogInfo() ) );

						var queryParams = ParseQueryParams( result );
						queryParams.TryGetValue( "oauth_token", out var token );
						queryParams.TryGetValue( "oauth_token_secret", out var tokenSecret );

						if (!( string.IsNullOrEmpty( token )
						       || string.IsNullOrEmpty( tokenSecret ) ))
							credentials = new OAuthCredentials( null, token, tokenSecret );
					}
					catch ( Exception exception )
					{
						var etsyException = new EtsyException( this.CreateMethodCallInfo( url, mark, additionalInfo : this.AdditionalLogInfo() ), exception );
						EtsyLogger.LogTraceException( etsyException );
					}

					return credentials;
				});
		}

		/// <summary>
		///	Returns temporary credentials and login url for customer
		/// </summary>
		/// <param name="scopes">Permissions</param>
		/// <returns></returns>
		public async Task<OAuthCredentials> GetTemporaryCredentials( string[] scopes )
		{
			Condition.Requires( scopes ).IsNotEmpty();

			var requestParameters = new Dictionary<string, string>
			{
				{ "scopes", string.Join(" ", scopes) },
				{ "oauth_callback", "oob" }
			};

			return await Policy.HandleResult<OAuthCredentials>( credentials => credentials == null )
				.WaitAndRetryAsync( RetryCount,
					retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
					(entityRaw, timeSpan, retryCount, context) =>
					{
						EtsyLogger.LogTraceRetryStarted(
							$"Request failed. Waiting {timeSpan} before next attempt. Retry attempt {retryCount}");
					} )
				.ExecuteAsync(async () =>
				{
					var mark = Mark.CreateNew();
					OAuthCredentials credentials = null;

					string absoluteUrl = BaseUrl + RequestTokenUrl;
					var oauthParameters = authenticator.GetOAuthRequestParameters( absoluteUrl, "GET", null, requestParameters );
					string url = authenticator.GetUrl( absoluteUrl, oauthParameters );

					try
					{
						EtsyLogger.LogStarted( this.CreateMethodCallInfo( url, mark, additionalInfo : this.AdditionalLogInfo() ) );

						HttpResponseMessage response = await httpClient.GetAsync( url ).ConfigureAwait( false );

						var result = response.Content.ReadAsStringAsync().Result;

						EtsyLogger.LogEnd( this.CreateMethodCallInfo( url, mark, methodResult: result.ToJson(), additionalInfo : this.AdditionalLogInfo() ) );

						HandleEtsyEndpointErrorResponse( result );

						if ( !string.IsNullOrEmpty( result )
						     && result.IndexOf( "login_url", StringComparison.InvariantCulture ) > -1 )
						{
							string loginUrl = Uri.UnescapeDataString( result.Replace( "login_url=", "" ) );

							string[] temp = loginUrl.Split( '?' );

							if ( temp.Length == 2 )
							{
								var queryParams = ParseQueryParams( temp[1] );
								queryParams.TryGetValue( "oauth_token", out var token );
								queryParams.TryGetValue( "oauth_token_secret", out var tokenSecret );

								if ( token != null && tokenSecret != null )
									credentials = new OAuthCredentials( loginUrl, token, tokenSecret );
							}
						}
					}
					catch ( Exception exception )
					{
						var etsyException = new EtsyException( this.CreateMethodCallInfo( url, mark, additionalInfo : this.AdditionalLogInfo() ), exception );
						EtsyLogger.LogTraceException( etsyException );
					}

					return credentials;
				});
		}

		/// <summary>
		///	Parses url query string into dictionary
		/// </summary>
		/// <param name="queryParams">Query parameters</param>
		/// <returns></returns>
		private Dictionary<string, string> ParseQueryParams(string queryParams)
		{
			var result = new Dictionary<string, string>();

			if (!string.IsNullOrEmpty(queryParams))
			{
				string[] keyValuePairs = queryParams.Split('&');

				foreach (string keyValuePair in keyValuePairs)
				{
					string[] keyValue = keyValuePair.Split('=');

					if (keyValue.Length == 2)
					{
						if (!result.TryGetValue(keyValue[0], out var tmp))
							result.Add(keyValue[0], keyValue[1]);
					}
				}
			}

			return result;
		}
	}
}
