using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EtsyAccess.Models;

namespace EtsyAccess.Services.Orders
{
	public class OrdersService : BaseService, IOrdersService
	{
		private readonly string ReceiptsUrl = "/v2/shops/{0}/receipts";

		public OrdersService( string consumerKey, string consumerSecret, string token, string tokenSecret ) : base( consumerKey, consumerSecret, token, tokenSecret )
		{ }

		public async Task<Receipt[]> GetOrdersAsync(DateTime startDate, DateTime endDate)
		{
			string url = String.Format(ReceiptsUrl, "19641381");

			var response = await base.GetAsync<Receipt>(url).ConfigureAwait(false);

			return response.Results;
		}

		Receipt[] IOrdersService.GetOrders(DateTime startDate, DateTime endDate)
		{
			return GetOrdersAsync(startDate, endDate).GetAwaiter().GetResult();
		}
	}
}
