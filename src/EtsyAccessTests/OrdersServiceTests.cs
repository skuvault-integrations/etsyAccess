using System;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;

namespace EtsyAccessTests
{
	public class OrdersServiceTests : BaseTest
	{
		[Test]
		public void GetOrders()
		{
			DateTime startDate = DateTime.Now.AddMonths(-1);
			DateTime endDate = DateTime.Now;

			var orders = this.EtsyOrdersService.GetOrders( startDate, endDate, CancellationToken.None );

			orders.Should().NotBeNullOrEmpty();
		}
	}
}
