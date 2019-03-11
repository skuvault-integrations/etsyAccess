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

		public AuthenticationService(string applicationKey, string sharedSecret)
		{
			_applicationKey = applicationKey;
			_sharedSecret = sharedSecret;

			_httpClient = new HttpClient
			{
				BaseAddress = new Uri(BaseUrl)
			};
		}

		/// <summary>
		///	Returns access token for making authorized API calls
		/// </summary>
		/// <returns></returns>
		public async Task< OAuthCredentials > GetPermanentCredentials( string temporaryToken, string temporaryTokenSecret, string verifierCode )
		{
			OAuthCredentials credentials = null;

			var requestParameters = new KeyValuePair<string, string>[]
			{
				new KeyValuePair<string, string>("oauth_token", temporaryToken),
				new KeyValuePair<string, string>("oauth_verifier", verifierCode),
			};

			var oauthParameters = GetOAuthRequestParameters(BaseUrl + AccessTokenUrl, "GET", temporaryTokenSecret, requestParameters);
			string url = GetUrl(AccessTokenUrl, oauthParameters);
			
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

			var requestParameters = new KeyValuePair<string, string>[]
				{ new KeyValuePair<string, string>("scopes", string.Join(" ", scopes)), };

			var oauthParameters = GetOAuthRequestParameters(BaseUrl + RequestTokenUrl, "GET", null, requestParameters);
			string url = GetUrl(RequestTokenUrl, oauthParameters);

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
		///	Returns OAuth 1.0 request parameters with signature
		/// </summary>
		/// <param name="url"></param>
		/// <param name="method"></param>
		/// <param name="tokenSecret"></param>
		/// <param name="extraRequestParameters"></param>
		/// <returns></returns>
		private Dictionary<string, string> GetOAuthRequestParameters(string url, string method, string tokenSecret, KeyValuePair<string, string>[] extraRequestParameters)
		{
			var requestParameters = new Dictionary<string, string>
			{
				{ "oauth_callback", "oob" },
				{ "oauth_consumer_key", _applicationKey },
				{ "oauth_nonce", GetRandomSessionNonce() },
				{ "oauth_signature_method", "HMAC-SHA1" },
				{ "oauth_timestamp", Misc.Misc.GetUnixEpochTime().ToString() },
				{ "oauth_version", "1.0" },
			};

			if (extraRequestParameters != null)
			{
				foreach(var keyValue in extraRequestParameters)
					requestParameters.Add(keyValue.Key, keyValue.Value);
			}

			string signature = GetOAuthSignature( url, method, tokenSecret, requestParameters );
			requestParameters.Add( "oauth_signature", signature );

			return requestParameters;
		}

		/// <summary>
		///	Returns signed request payload by using HMAC-SHA1
		/// </summary>
		/// <param name="url"></param>
		/// <param name="urlMethod"></param>
		/// <param name="tokenSecret"></param>
		/// <param name="requestParameters"></param>
		/// <returns></returns>
		private string GetOAuthSignature( string url, string urlMethod, string tokenSecret, Dictionary< string, string > requestParameters )
		{
			string signature = null;

			string urlEncoded = Misc.Misc.EscapeUriDataStringRfc3986(url);
			string encodedParameters = Misc.Misc.EscapeUriDataStringRfc3986(string.Join("&",
				requestParameters.OrderBy(kv => kv.Key).Select(item =>
					($"{ Misc.Misc.EscapeUriDataStringRfc3986(item.Key)}={Misc.Misc.EscapeUriDataStringRfc3986(item.Value)}"))));
			
			string baseString = $"{urlMethod.ToUpper()}&{urlEncoded}&{encodedParameters}";

			HMACSHA1 hmacsha1 = new HMACSHA1( Encoding.ASCII.GetBytes( _sharedSecret + "&" + (string.IsNullOrEmpty(tokenSecret) ? "" : tokenSecret) ) );
			byte[] data = Encoding.ASCII.GetBytes( baseString );

			using (var stream = new MemoryStream(data))
			{
				signature = Convert.ToBase64String(hmacsha1.ComputeHash(stream));
			}

			return signature;
		}

		/// <summary>
		///	Generates random nonce for each request
		/// </summary>
		/// <returns></returns>
		private string GetRandomSessionNonce()
		{
			return Guid.NewGuid().ToString().Replace("-", "").Substring(0, 11).ToUpper();
		}

		/// <summary>
		///	Returns url with query parameters
		/// </summary>
		/// <param name="baseUrl"></param>
		/// <param name="requestParameters"></param>
		/// <returns></returns>
		private string GetUrl(string baseUrl, Dictionary<string, string> requestParameters)
		{
			string url = baseUrl;

			var paramsBuilder = new StringBuilder();

			foreach (var kv in requestParameters)
			{
				if (paramsBuilder.Length > 0)
					paramsBuilder.Append("&");

				paramsBuilder.Append($"{kv.Key}={kv.Value}");
			}

			url += "?" + paramsBuilder.ToString();

			return url;
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
