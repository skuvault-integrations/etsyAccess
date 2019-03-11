using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using EtsyAccess.Models;
using EtsyAccess.Services.Authentication;
using Newtonsoft.Json;

namespace EtsyAccess.Services
{
	public class BaseService
	{
		private const string BaseApiUrl = "https://openapi.etsy.com";

		private readonly HttpClient _httpClient;
		private readonly OAuthenticator _authenticator;

		public BaseService( string consumerKey, string consumerSecret, string token, string tokenSecret )
		{
			_httpClient = new HttpClient()
			{
				BaseAddress = new Uri(BaseApiUrl)
			};

			_authenticator = new OAuthenticator( consumerKey, consumerSecret, token, tokenSecret);
		}

		public async Task<EtsyResponse<T>> GetAsync<T>(string url)
		{
			url = _authenticator.GetUriWithOAuthQueryParameters(BaseApiUrl + url);

			var response = await _httpClient.GetAsync(url).ConfigureAwait(false);

			string responseStr = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

			var data = JsonConvert.DeserializeObject<EtsyResponse<T>>(responseStr);

			return data;
		}
	}
}
