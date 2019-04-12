using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
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
			int quantity = 12;

			this.EtsyItemsService.UpdateSkuQuantity( sku, quantity, CancellationToken.None );

			// assert
			var inventory = this.EtsyItemsService.GetListingProductBySku( sku, CancellationToken.None ).GetAwaiter().GetResult();

			inventory.Should().NotBeNull();
			inventory.Offerings.Should().NotBeNullOrEmpty();
			inventory.Offerings.First().Quantity.Should().Be( quantity );
		}

		[ Test ]
		public void UpdateSkuQuantityToZero()
		{
			string sku = "testSku1";
			int quantity = 0;

			this.EtsyItemsService.UpdateSkuQuantity( sku, quantity, CancellationToken.None );

			// assert
			var inventory = this.EtsyItemsService.GetListingProductBySku( sku, CancellationToken.None ).GetAwaiter().GetResult();

			inventory.Should().NotBeNull();
			inventory.Offerings.Should().NotBeNullOrEmpty();
			inventory.Offerings.First().Quantity.Should().Be( quantity );
		}

		[ Test ]
		public void UpdateSkusQuantities()
		{
			string sku = "testSku1";
			int skuQuantity = 6;
			string sku2 = "B07DBJSDPN-20BR";
			int sku2Quantity = 98;

			var quantities = new Dictionary<string, int>
			{
				{ sku, skuQuantity },
				{ sku2, sku2Quantity }
			};

			this.EtsyItemsService.UpdateSkusQuantityAsync(quantities, CancellationToken.None).GetAwaiter().GetResult();

			// assert
			var skuInventory = this.EtsyItemsService.GetListingProductBySku( sku, CancellationToken.None ).GetAwaiter().GetResult();

			skuInventory.Should().NotBeNull();
			skuInventory.Offerings.Should().NotBeNullOrEmpty();
			skuInventory.Offerings.First().Quantity.Should().Be( skuQuantity );

			var sku2Inventory = this.EtsyItemsService.GetListingProductBySku( sku2, CancellationToken.None ).GetAwaiter().GetResult();

			sku2Inventory.Should().NotBeNull();
			sku2Inventory.Offerings.Should().NotBeNullOrEmpty();
			sku2Inventory.Offerings.First().Quantity.Should().Be( sku2Quantity );
		}
	}
}
