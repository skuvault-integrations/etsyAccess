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

namespace EtsyAccess.Services
{
	public class BaseService
	{
		private const string BaseApiUrl = "https://openapi.etsy.com";
		private readonly string ShopsInfoUrl = "/v2/shops/{0}";
		private readonly string _shopName;

		private readonly HttpClient _httpClient;
		private readonly OAuthenticator _authenticator;
		private Func< string > _additionalLogInfo;

		/// <summary>
		///	Extra logging information
		/// </summary>
		public Func<string> AdditionalLogInfo
		{
			get { return this._additionalLogInfo ?? ( () => string.Empty ); }
			set => _additionalLogInfo = value;
		}

		public BaseService( string shopName, string consumerKey, string consumerSecret, string token, string tokenSecret )
		{
			_shopName = shopName;

			_httpClient = new HttpClient()
			{
				BaseAddress = new Uri( BaseApiUrl )
			};

			_authenticator = new OAuthenticator( consumerKey, consumerSecret, token, tokenSecret);
		}

		/// <summary>
		///	Returns current shop info
		/// </summary>
		public async Task<Shop> GetShopInfo()
		{
			IEnumerable< Shop > response = null;
			string url = String.Format(ShopsInfoUrl, _shopName);

			try
			{
				EtsyLogger.LogStarted( this.CreateMethodCallInfo( mark : "", additionalInfo : this.AdditionalLogInfo(), methodParameters : url ) );

				response = await GetEntitiesAsync< Shop >( url ).ConfigureAwait( false );

				EtsyLogger.LogEnd( this.CreateMethodCallInfo( mark : "", additionalInfo : this.AdditionalLogInfo(), methodParameters : url ) );
			}
			catch ( Exception exception )
			{
				var etsyException = new EtsyException( this.CreateMethodCallInfo( mark : "", additionalInfo : this.AdditionalLogInfo(), methodParameters : "" ), exception );
				EtsyLogger.LogTraceException( etsyException );
				throw etsyException;
			}

			return response.FirstOrDefault();
		}

		/// <summary>
		///	Returns entities asynchronously
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="result"></param>
		/// <param name="url"></param>
		/// <returns></returns>
		public async Task< IEnumerable< T > > GetEntitiesAsync< T >( string url, List< T > result = null )
		{
			url = _authenticator.GetUriWithOAuthQueryParameters(BaseApiUrl + url);

			try
			{
				var httpResponse = await _httpClient.GetAsync(url).ConfigureAwait( false );
				string content = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait( false );

				// handle Etsy error
				HandleEtsyEndpointErrorResponse( content );

				var response = JsonConvert.DeserializeObject< EtsyResponse< T > >( content );

				if (result == null)
					result = new List<T>();

				result.AddRange(response.Results);

				// handle pagination
				if ( response.Pagination.NextPage != null 
					&& response.Pagination.NextOffset != null )
				{
					int offset = response.Pagination.NextOffset.Value;

					url = url.Replace(BaseApiUrl, "");

					if (url.Contains('?'))
						url += "&";
					else
						url += "?";

					url += url.Contains("offset") ? url.Replace($"offset={offset - 1}", $"offset={offset}") : $"offset={offset}";

					await GetEntitiesAsync( url, result ).ConfigureAwait(false);
				}
			}
			catch ( Exception exception )
			{
				var etsyException = new EtsyException( this.CreateMethodCallInfo( mark : "", additionalInfo : this.AdditionalLogInfo(), methodParameters : "" ), exception );
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
		/// <returns></returns>
		public async Task< T > GetEntityAsync< T >( string url )
		{
			url = _authenticator.GetUriWithOAuthQueryParameters( BaseApiUrl + url );

			try
			{
				var httpResponse = await _httpClient.GetAsync( url ).ConfigureAwait( false );
				string content = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait( false );

				// handle Etsy error
				HandleEtsyEndpointErrorResponse( content );

				var response = JsonConvert.DeserializeObject< EtsyResponseSingleEntity< T > >( content );

				return response.Result;
			}
			catch ( Exception exception )
			{
				var etsyException = new EtsyException( this.CreateMethodCallInfo( mark : "", additionalInfo : this.AdditionalLogInfo(), methodParameters : "" ), exception );
				EtsyLogger.LogTraceException( etsyException );
				throw etsyException;
			}
		}

		/// <summary>
		///	Updates data
		/// </summary>
		/// <param name="payload"></param>
		/// <param name="url"></param>
		/// <returns></returns>
		public async Task PutAsync( string url, Dictionary<string, string> payload )
		{
			var content = new FormUrlEncodedContent( payload );
			url = _authenticator.GetUriWithOAuthQueryParameters( BaseApiUrl + url, "PUT", payload );

			try
			{
				var response = await _httpClient.PutAsync( url, content ).ConfigureAwait( false );
				string responseStr = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

				// handle server response maybe some error happened
				HandleEtsyEndpointErrorResponse( responseStr );

				if ( response.StatusCode != HttpStatusCode.OK )
					throw new EtsyException( responseStr );
			}
			catch (Exception exception)
			{
				var etsyException = new EtsyException( this.CreateMethodCallInfo( mark : "", additionalInfo : this.AdditionalLogInfo(), methodParameters : "" ), exception );
				EtsyLogger.LogTraceException( etsyException );
				throw etsyException;
			}
		}

		/// <summary>
		///	Handles Etsy server error responses
		/// </summary>
		/// <param name="response"></param>
		private void HandleEtsyEndpointErrorResponse( string response )
		{
			if ( string.IsNullOrEmpty(response) )
				return;

			if ( response.IndexOf("signature_invalid", StringComparison.InvariantCulture ) > -1)
				throw new EtsyInvalidSignatureException( response );
		}

		protected string CreateMethodCallInfo( string methodParameters = "", string mark = "", string errors = "", string methodResult = "", string additionalInfo = "", [ CallerMemberName ] string memberName = "" )
		{
			var str = string.Format(
				"{{MethodName:{0}, Mark:'{3}', MethodParameters:{2}{4}{5}",
				memberName,
				methodParameters,
				mark,
				string.IsNullOrWhiteSpace( errors ) ? string.Empty : ", Errors:" + errors,
				string.IsNullOrWhiteSpace( methodResult ) ? string.Empty : ", Result:" + methodResult,
				string.IsNullOrWhiteSpace( additionalInfo ) ? string.Empty : ", " + additionalInfo
			);
			return str;
		}
	}
}
