using System;
using System.Collections.Generic;
using System.Text;
using CuttingEdge.Conditions;

namespace EtsyAccess.Models.Configuration
{
	public sealed class EtsyConfig
	{
		/// <summary>
		///	Tenant shop id
		/// </summary>
		public string ShopName { get; private set; }
		/// <summary>
		///	Consumer key in OAuth 1.0 terms
		/// </summary>
		public string Token { get; private set; }
		/// <summary>
		///	Permanent token secret
		/// </summary>
		public string TokenSecret { get; private set; }

		/// <summary>
		///	API url
		/// </summary>
		public readonly string ApiBaseUrl = "https://openapi.etsy.com";

		/// <summary>
		///	Max retry attempts if we get network errors ( total time for attempts over 14 seconds)
		/// 
		/// </summary>
		public readonly int RetryAttempts = 4;

		/// <summary>
		///	Request timeout
		/// </summary>
		public readonly int RequestTimeoutMs = 30 * 1000;

		/// <summary>
		///	Throttling, max requests per interval
		/// </summary>
		public readonly int ThrottlingMaxRequestsPerRestoreInterval = 10;

		/// <summary>
		///	Throttling time interval in seconds
		/// </summary>
		public readonly int ThrottlingRestorePeriodInSeconds = 1;

		/// <summary>
		///	Throttling, max retry attempts if API limits exceeded
		/// </summary>
		public readonly int ThrottlingMaxRetryAttempts = 10;

		public EtsyConfig( string shopName, string token, string tokenSecret )
		{
			Condition.Requires( shopName ).IsNotNullOrEmpty();
			Condition.Requires( token ).IsNotNullOrEmpty();
			Condition.Requires( tokenSecret ).IsNotNullOrEmpty();

			ShopName = shopName;
			Token = token;
			TokenSecret = tokenSecret;
		}
	}

	public class EtsyEndPoint
	{
		public static readonly string GetReceiptsUrl = "/v2/shops/{0}/receipts?includes=Transactions,Listings,Country&limit=100";
		public static readonly string GetShopInfoUrl = "/v2/shops/{0}";
		public static readonly string GetShopActiveListingsUrl = "/v2/shops/{0}/listings/active?limit=100";
		public static readonly string GetListingInventoryUrl = "/v2/listings/{0}/inventory?write_missing_inventory=true";
		public static readonly string UpdateListingInventoryUrl = "/v2/listings/{0}/inventory";
		public static readonly string GetRequestTokenUrl = "/v2/oauth/request_token";
		public static readonly string GetAccessTokenUrl = "/v2/oauth/access_token";
	}
}
