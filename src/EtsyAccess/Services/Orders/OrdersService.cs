using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EtsyAccess.Misc;
using EtsyAccess.Models;

namespace EtsyAccess.Services.Orders
{
	public class OrdersService : BaseService, IOrdersService
	{
		private readonly string ReceiptsUrl = "/v2/shops/{0}/receipts?includes=Transactions,Listings";
		private readonly string ShopsInfoUrl = "/v2/shops/{0}";
		private readonly string _shopName;

		public OrdersService(string consumerKey, string consumerSecret, string token, string tokenSecret,
			string shopName) : base(consumerKey, consumerSecret, token, tokenSecret)
		{
			_shopName = shopName;
		}

		/// <summary>
		///	Returns receipts that were changed in the specified period
		/// </summary>
		/// <param name="startDate"></param>
		/// <param name="endDate"></param>
		/// <returns></returns>
		public async Task<Receipt[]> GetOrdersAsync(DateTime startDate, DateTime endDate)
		{
			var shop = await GetShopInfo().ConfigureAwait( false );

			long minLastModified = startDate.FromUtcTimeToEpoch();
			long maxLastModified = endDate.FromUtcTimeToEpoch();

			string url = String.Format(ReceiptsUrl + "&min_last_modified={1}&max_last_modified={2}", shop.Id,
				minLastModified, maxLastModified);

			var response = await base.GetAsync<Receipt>(url).ConfigureAwait(false);

			return response.Results;
		}

		Receipt[] IOrdersService.GetOrders(DateTime startDate, DateTime endDate)
		{
			return GetOrdersAsync(startDate, endDate).GetAwaiter().GetResult();
		}

		/// <summary>
		///	Returns current shop info
		/// </summary>
		public async Task<Shop> GetShopInfo()
		{
			string url = String.Format(ShopsInfoUrl, _shopName);
			var response = await base.GetAsync<Shop>(url).ConfigureAwait(false);

			return response.Results.FirstOrDefault();
		}
	}
}
