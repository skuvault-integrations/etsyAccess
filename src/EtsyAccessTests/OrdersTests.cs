using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace EtsyAccessTests
{
	public class OrdersTests : BaseTest
	{
		[ Test ]
		public void GetOrders()
		{
			DateTime startDate = DateTime.Now.AddMonths(-1);
			DateTime endDate = DateTime.Now;

			var orders = this.OrdersService.GetOrders(startDate, endDate);

			orders.Should().NotBeNullOrEmpty();
		}

		[ Test ]
		public void GetShopInfoByName()
		{
			var shop = this.OrdersService.GetShopInfo().GetAwaiter().GetResult();

			shop.Should().NotBeNull();
		}
	}
}
