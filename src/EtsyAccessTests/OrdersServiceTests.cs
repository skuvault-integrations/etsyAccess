using System;
using System.Linq;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;

namespace EtsyAccessTests
{
	public class OrdersServiceTests : BaseTest
	{
		[ Test ]
		public void GetOrders()
		{
			var startDate = DateTime.Now.AddDays( -1 );
			var endDate = DateTime.Now;

			var orders = this.EtsyOrdersService.GetOrders( startDate, endDate, CancellationToken.None );

			orders.Should().NotBeNullOrEmpty();
		}

		[ Test ]
		public void GetOrdersWithShipments()
		{
			var startDate = DateTime.Now.AddDays( -30 );
			var endDate = DateTime.Now;

			var orders = this.EtsyOrdersService.GetOrders( startDate, endDate, CancellationToken.None );
			var ordersWithShipments = orders.Where( o => o.Shipments.Any() ).ToList();

			ordersWithShipments.Count.Should().BeGreaterThan( 0 );
		}
	}
}