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
using NLog;
using NLog.Fluent;
using NLog.LayoutRenderers.Wrappers;

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

	public class AuthenticationService : IAuthenticationService
	{
		private const string BaseUrl = "https://openapi.etsy.com/";
		private const string RequestTokenUrl = "v2/oauth/request_token";
		private const string AccessTokenUrl = "v2/oauth/access_token";

		private readonly string _applicationKey;
		private readonly string _sharedSecret;

		private readonly HttpClient _httpClient;
		private readonly OAuthenticator _authenticator;

		public AuthenticationService(string applicationKey, string sharedSecret)
		{
			_applicationKey = applicationKey;
			_sharedSecret = sharedSecret;

			_httpClient = new HttpClient
			{
				BaseAddress = new Uri(BaseUrl)
			};

			_authenticator = new OAuthenticator( applicationKey, sharedSecret );
		}

		/// <summary>
		///	Returns access token for making authorized API calls
		/// </summary>
		/// <returns></returns>
		public async Task< OAuthCredentials > GetPermanentCredentials( string temporaryToken, string temporaryTokenSecret, string verifierCode )
		{
			OAuthCredentials credentials = null;

			var requestParameters = new Dictionary<string, string>
			{
				{ "oauth_token", temporaryToken },
				{ "oauth_verifier", verifierCode }
			};

			string absoluteUrl = BaseUrl + AccessTokenUrl;
			var oauthParameters = _authenticator.GetOAuthRequestParameters(absoluteUrl, "GET", temporaryTokenSecret, requestParameters);
			string url = _authenticator.GetUrl(absoluteUrl, oauthParameters);
			
			HttpResponseMessage response = await _httpClient.GetAsync( url ).ConfigureAwait( false );
			var result = response.Content.ReadAsStringAsync().Result;

			var queryParams = ParseQueryParams(result);
			queryParams.TryGetValue("oauth_token", out var token);
			queryParams.TryGetValue("oauth_token_secret", out var tokenSecret);

			if (!(string.IsNullOrEmpty(token)
				|| string.IsNullOrEmpty(tokenSecret)))
				credentials = new OAuthCredentials(null, token, tokenSecret);

			return credentials;
		}

		/// <summary>
		///	Returns temporary credentials and login url for customer
		/// </summary>
		/// <param name="scopes">Permissions</param>
		/// <returns></returns>
		public async Task<OAuthCredentials> GetTemporaryCredentials(string[] scopes)
		{
			OAuthCredentials credentials = null;

			var requestParameters = new Dictionary<string, string>
			{
				{ "scopes", string.Join(" ", scopes) },
				{ "oauth_callback", "oob" }
			};

			string absoluteUrl = BaseUrl + RequestTokenUrl;
			var oauthParameters = _authenticator.GetOAuthRequestParameters(absoluteUrl, "GET", null, requestParameters);
			string url = _authenticator.GetUrl(absoluteUrl, oauthParameters);

			HttpResponseMessage response = await _httpClient.GetAsync( url ).ConfigureAwait( false );
			var result = response.Content.ReadAsStringAsync().Result;

			if (!string.IsNullOrEmpty(result)
				&& result.IndexOf("login_url", StringComparison.InvariantCulture) > -1)
			{
				string loginUrl = Uri.UnescapeDataString(result.Replace("login_url=", ""));

				string[] temp = loginUrl.Split('?');

				if (temp.Length == 2)
				{
					var queryParams = ParseQueryParams(temp[1]);
					queryParams.TryGetValue("oauth_token", out var token);
					queryParams.TryGetValue("oauth_token_secret", out var tokenSecret);

					if (token != null && tokenSecret != null)
						credentials = new OAuthCredentials(loginUrl, token, tokenSecret);
				}

			}

			return credentials;
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
