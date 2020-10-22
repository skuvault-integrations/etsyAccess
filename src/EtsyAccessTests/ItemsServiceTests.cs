using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using EtsyAccess.Exceptions;
using EtsyAccess.Models.Requests;
using FluentAssertions;
using NUnit.Framework;

namespace EtsyAccessTests
{
	public class ItemsServiceTests : BaseTest
	{
		private const string Sku = "testsku1";

		[ Test ]
		[ Ignore( "Creates listings in the real store" ) ]
		public void GenerateListings()
		{
			var requests = new List< PostListingRequest >();
			int listingsTotal = 200;
			int i = 2;

			while ( i < listingsTotal )
			{
				requests.Add( new PostListingRequest( quantity: i + 1,
										  title: "testSku" + i.ToString(),
										  description: "Test listing #" + i.ToString(),
										  price: 1.0f,
										  whoMade: WhoMadeEnum.i_did,
										  isSupply: false,
										  whenMade: "2010_2019",
										  shippingTemplateId: 76153032027 )
				{
					State = StateEnum.draft // otherwise you may pay listing fees
				});

				i++;
			}

			foreach( var request in requests )
				this.EtsyItemsService.CreateListing( request, CancellationToken.None ).GetAwaiter().GetResult();
		}

		[ Test ]
		public void GetDraftListings()
		{
			var draftListings = this.EtsyItemsService.GetDraftListings( CancellationToken.None ).Result;

			draftListings.Should().NotBeNullOrEmpty();
		}

		[ Test ]
		public void GetListingsGetListingProductBySkuBySku()
		{
			var listing = this.EtsyItemsService.GetListingProductBySku( Sku, CancellationToken.None ).Result;

			Assert.IsNotNull( listing );
			Assert.AreEqual( Sku, listing.Sku );
		}

		[ Test ]
		public void GetListingsBySkus()
		{
			var listing = this.EtsyItemsService.GetListingsBySkus( new List<string> { Sku }, CancellationToken.None ).Result.ToList();

			Assert.IsTrue( listing.Any() );
			Assert.AreEqual( Sku, listing[0].Sku[0] );
		}

		[ Test ]
		public void UpdateSkuQuantity()
		{
			int quantity = 12;

			this.EtsyItemsService.UpdateSkuQuantity( Sku, quantity, CancellationToken.None );

			// assert
			var inventory = this.EtsyItemsService.GetListingProductBySku( Sku, CancellationToken.None ).GetAwaiter().GetResult();

			inventory.Should().NotBeNull();
			inventory.Offerings.Should().NotBeNullOrEmpty();
			inventory.Offerings.First().Quantity.Should().Be( quantity );
		}

		[ Test ]
		public void UpdateSkuQuantityToZero()
		{
			int quantity = 0;
			const string message = "offering must have quantity greater than 0";

			// assert
			var etsyException = Assert.Throws< EtsyException >( () =>
			{
				this.EtsyItemsService.UpdateSkuQuantity( Sku, quantity, CancellationToken.None );
			});

			Assert.That( etsyException.ToString().Contains( message ) );
		}

		[ Test ]
		public void UpdateSkuQuantityWithQuoteInTheOptionName()
		{
			int quantity = 2;
			var testSku = "testsku2-1";

			this.EtsyItemsService.UpdateSkuQuantity( testSku, quantity, CancellationToken.None );

			// assert
			var inventory = this.EtsyItemsService.GetListingProductBySku( testSku, CancellationToken.None ).GetAwaiter().GetResult();

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

		[ Test ]
		// This will take awhile
		public void UpdateSkusQuantities_Over30Variations_ShouldUpdateThem()
		{
			const string sku = "testsku1-";
			const int minQuantity = 6;
			var quantities = new Dictionary< string, int >();
			var rand = new Random( DateTime.Now.Millisecond );
			const int variationCount = 35;
			for( var i = 1; i <= variationCount; i++ )
			{
				quantities.Add( sku + i, minQuantity + rand.Next( 1, 50 ) );
			}
			var cancellationTokenSource = new CancellationTokenSource();

			this.EtsyItemsService.UpdateSkusQuantityAsync( quantities, cancellationTokenSource.Token ).GetAwaiter().GetResult();

			var skuInventory = this.EtsyItemsService.GetListingProductBySku( sku, CancellationToken.None ).GetAwaiter().GetResult();
			skuInventory.Offerings.Length.Should().BeGreaterOrEqualTo( variationCount );
			skuInventory.Offerings.First().Quantity.Should().Be( quantities.First().Value );
		}
	}
}