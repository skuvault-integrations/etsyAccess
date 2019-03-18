using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EtsyAccess.Exceptions;
using EtsyAccess.Misc;
using EtsyAccess.Models;
using EtsyAccess.Models.Requests;
using Newtonsoft.Json;

namespace EtsyAccess.Services.Items
{
	public class ItemsService : BaseService, IItemsService
	{
		private readonly string UpdateListingProductQuantityUrl = "/v2/listings/{0}/inventory";
		// for test purposes: in prod replace "inactive"
		private readonly string GetShopActiveListingsUrl = "/v2/shops/{0}/listings/inactive?limit=100";
		private readonly string GetListingProductsInventoryUrl = "/v2/listings/{0}/inventory?write_missing_inventory=true";

		public ItemsService( string consumerKey, string consumerSecret, string token, string tokenSecret, int shopId ) : base( consumerKey, consumerSecret, token, tokenSecret, shopId )
		{ }

		public void UpdateSkuQuantity(string sku, int quantity)
		{
			UpdateSkuQuantityAsync( sku, quantity ).GetAwaiter().GetResult();
		}

		/// <summary>
		///	Updates sku's quantity
		/// </summary>
		/// <param name="sku"></param>
		/// <param name="quantity"></param>
		/// <returns></returns>
		public async Task UpdateSkuQuantityAsync( string sku, int quantity )
		{
			var mark = Mark.CreateNew();

			// get all listings that have products with specified sku
			var listings = await GetListingsBySku( sku ).ConfigureAwait( false );
			var listing = listings.FirstOrDefault();

			if ( listing == null)
				return;

			// get listing inventory
			var listingInventory = await GetListingInventoryBySku( listing, sku ).ConfigureAwait( false );
						
			await UpdateSkuQuantityAsync( listing, listingInventory, sku, quantity ).ConfigureAwait( false );
		}

		/// <summary>
		///	Updates sku quantity asynchronously
		/// </summary>
		/// <param name="listing"></param>
		/// <param name="inventory"></param>
		/// <param name="sku"></param>
		/// <param name="quantity"></param>
		/// <returns></returns>
		private async Task UpdateSkuQuantityAsync( Listing listing, ListingInventory inventory, string sku, int quantity)
		{
			var mark = Mark.CreateNew();

			List< UpdateInventoryRequest > updateInventoryRequest = new List< UpdateInventoryRequest >();

			// we should also add all product variations to request
			foreach ( var product in inventory.Products )
			{
				var productOffering = product.Offerings.FirstOrDefault();

				if ( productOffering == null )
					continue;

				int productQuantity = productOffering.Quantity;

				if ( product.Sku != null && product.Sku.ToLower().Equals( sku.ToLower() ) )
					productQuantity = quantity;

				updateInventoryRequest.Add( new UpdateInventoryRequest()
				{
					ProductId = product.Id,
					Sku = product.Sku,
					PropertyValues = product.PropertyValues,
					// currently each product has one offering
					ListingOffering = new ListingOfferingRequest[]
					{
						new ListingOfferingRequest()
						{
							Id = productOffering.Id,
							Quantity = productQuantity,
							Price = (float)(productOffering.Price.Amount * 1.0 / productOffering.Price.Divisor)
						}
					}
				});
			}

			string url = String.Format( UpdateListingProductQuantityUrl, listing.Id );

			try
			{
				EtsyLogger.LogStarted( this.CreateMethodCallInfo( url, mark, additionalInfo : this.AdditionalLogInfo() ) );

				var payload = new Dictionary<string, string>
				{
					{ "products", JsonConvert.SerializeObject( updateInventoryRequest.ToArray() ) }
				};

				if (inventory.PriceOnProperty.Length != 0)
					payload.Add("price_on_property", string.Join( ",", inventory.PriceOnProperty ) );

				if (inventory.QuantityOnProperty.Length != 0)
					payload.Add("quantity_on_property", string.Join( ",", inventory.QuantityOnProperty ) );

				if (inventory.SkuOnProperty.Length != 0)
					payload.Add("sku_on_property", string.Join( ",", inventory.SkuOnProperty ) );

				await base.PutAsync( url, payload, mark ).ConfigureAwait( false );

				EtsyLogger.LogEnd( this.CreateMethodCallInfo( url, mark, additionalInfo : this.AdditionalLogInfo() ) );
			}
			catch ( Exception exception )
			{
				var etsyException = new EtsyException( this.CreateMethodCallInfo( url, mark, additionalInfo : this.AdditionalLogInfo() ), exception );
				EtsyLogger.LogTraceException( etsyException );
				throw etsyException;
			}
		}

		/// <summary>
		///	Updates skus quantities
		/// </summary>
		/// <param name="skusQuantities"></param>
		/// <returns></returns>
		public async Task UpdateSkusQuantityAsync( Dictionary<string, int> skusQuantities )
		{
			if ( skusQuantities == null || skusQuantities.Count == 0 )
				return;

			var listings = await GetListingsBySkus( skusQuantities.Keys ).ConfigureAwait( false );

			foreach ( var skuQuantity in skusQuantities )
			{
				string sku = skuQuantity.Key;
				int quantity = skuQuantity.Value;

				var listing = listings.Where( item => item.Sku.Select( s => s.ToLower() ).Contains( sku.ToLower() ) ).FirstOrDefault();

				if ( listing == null )
					continue;

				var listingInventory = await GetListingInventoryBySku( listing, sku ).ConfigureAwait( false );

				if ( listingInventory != null )
					await UpdateSkuQuantityAsync( listing, listingInventory, sku, quantity );
			}
		}

		/// <summary>
		///	Returns listings' product by sku
		/// </summary>
		/// <param name="sku"></param>
		/// <returns></returns>
		public async Task< ListingProduct > GetListingProductBySku( string sku )
		{
			var inventory = await GetListingInventoryBySku( sku ).ConfigureAwait(false);

			return inventory.Products
					.FirstOrDefault( product => product.Sku != null && product.Sku.ToLower().Equals( sku.ToLower() ) );
		}

		/// <summary>
		///	Returns sku inventory
		/// </summary>
		/// <param name="sku"></param>
		/// <returns></returns>
		public async Task< ListingInventory > GetListingInventoryBySku( string sku )
		{
			ListingInventory listingInventory = null;

			// get all listings that have products with specified sku
			var listings = await GetListingsBySku( sku ).ConfigureAwait( false );
			var listing = listings.FirstOrDefault();

			if (listing != null)
			{
				// get listing's product
				listingInventory = await GetListingInventoryBySku( listing, sku ).ConfigureAwait( false );
			}

			return listingInventory;
		}

		
		/// <summary>
		///	Returns listing's products
		/// </summary>
		/// <param name="listing"></param>
		/// <param name="sku"></param>
		/// <returns></returns>
		public async Task< ListingInventory > GetListingInventoryBySku( Listing listing, string sku )
		{
			var mark = Mark.CreateNew();
			string url = String.Format( GetListingProductsInventoryUrl, listing.Id );

			try
			{
				EtsyLogger.LogStarted( this.CreateMethodCallInfo( url, mark, additionalInfo : this.AdditionalLogInfo() ) );

				var inventory = await GetEntityAsync< ListingInventory >( url, mark ).ConfigureAwait( false );

				EtsyLogger.LogEnd( this.CreateMethodCallInfo( url, mark, methodResult: inventory.ToJson(), additionalInfo : this.AdditionalLogInfo() ) );

				return inventory;
			}
			catch ( Exception exception )
			{
				var etsyException = new EtsyException( this.CreateMethodCallInfo( url, mark, additionalInfo : this.AdditionalLogInfo() ), exception );
				EtsyLogger.LogTraceException( etsyException );
				throw etsyException;
			}
		}

		/// <summary>
		///	Returns shop's active listings
		/// </summary>
		/// <param name="sku">Product sku</param>
		/// <returns></returns>
		public async Task < IEnumerable< Listing > > GetListingsBySku( string sku )
		{
			return await GetListingsBySkus( new string[] { sku } ).ConfigureAwait( false );
		}

		/// <summary>
		///	Returns listings with specified skus
		/// </summary>
		/// <param name="skus"></param>
		/// <returns></returns>
		public async Task< IEnumerable< Listing > > GetListingsBySkus( IEnumerable< string > skus )
		{
			var mark = Mark.CreateNew();
			string url = String.Format( GetShopActiveListingsUrl, shopId );

			try
			{
				EtsyLogger.LogStarted( this.CreateMethodCallInfo( url, mark, additionalInfo : this.AdditionalLogInfo() ) );

				var listings = await GetEntitiesAsync< Listing >( url, mark: mark ).ConfigureAwait( false );

				EtsyLogger.LogEnd( this.CreateMethodCallInfo( url, mark, methodResult: listings.ToJson(), additionalInfo : this.AdditionalLogInfo() ) );

				return listings.Where( listing => listing.Sku.Select( sku => sku.ToLower()).Intersect( skus.Select( sku => sku.ToLower() ) ).Any() ).ToArray();

			}
			catch (Exception exception)
			{
				var etsyException = new EtsyException( this.CreateMethodCallInfo( url, mark, additionalInfo : this.AdditionalLogInfo() ), exception );
				EtsyLogger.LogTraceException( etsyException );
				throw etsyException;
			}
		}

	}
}
