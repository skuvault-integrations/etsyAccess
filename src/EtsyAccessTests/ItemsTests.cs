using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace EtsyAccessTests
{
	public class ItemsTests : BaseTest
	{
		[ Test ]
		public void UpdateSkuQuantity()
		{
			string sku = "testSku1";
			int quantity = 25;

			this.ItemsService.UpdateSkuQuantity( sku, quantity );

			// assert
			var inventory = this.ItemsService.GetListingProductBySku( sku ).GetAwaiter().GetResult();

			inventory.Should().NotBeNull();
			inventory.Offerings.Should().NotBeNullOrEmpty();
			inventory.Offerings.First().Quantity.Should().Be( quantity );
		}
	}
}
