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
		private readonly string GetShopActiveListingsUrl = "/v2/shops/{0}/listings/inactive";
		private readonly string GetListingProductsInventoryUrl = "/v2/listings/{0}/inventory";

		public ItemsService( string shopName, string consumerKey, string consumerSecret, string token, string tokenSecret ) : base( shopName, consumerKey, consumerSecret, token, tokenSecret )
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
			var shop = await GetShopInfo().ConfigureAwait( false );

			// get all listings that have products with specified sku
			var listings = await GetListingsBySku( shop, sku ).ConfigureAwait( false );
			var listing = listings.FirstOrDefault();

			if ( listing == null)
				return;

			// get listing's product
			var product = await GetListingProductBySku( listing, sku ).ConfigureAwait( false );
						
			// lets update product offering
			// currently each product has one offering
			var productOffering = product.Offerings.First();
			var updateProductQuantityRequest = new UpdateInventoryRequest[]
			{
				new UpdateInventoryRequest()
				{
					ProductId = product.Id,
					Sku = product.Sku,
					PropertyValues = new string[] {},
					ListingOffering = new ListingOfferingRequest[]
					{
						new ListingOfferingRequest()
						{
							Id = productOffering.Id,
							Quantity = quantity,
							Price = (float)(productOffering.Price.Amount * 1.0 / productOffering.Price.Divisor)
						}
					}
				}
			}; 

			string url = String.Format( UpdateListingProductQuantityUrl, listing.Id );

			try
			{
				EtsyLogger.LogStarted( this.CreateMethodCallInfo( url, mark, additionalInfo : this.AdditionalLogInfo() ) );

				var payload = new Dictionary<string, string>
				{
					{ "products", JsonConvert.SerializeObject( updateProductQuantityRequest ) }
				};

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
		///	Returns sku inventory
		/// </summary>
		/// <param name="sku"></param>
		/// <returns></returns>
		public async Task< ListingProduct > GetListingProductBySku( string sku )
		{
			ListingProduct listingProduct = null;

			var shop = await GetShopInfo().ConfigureAwait( false );

			if ( shop != null )
			{
				// get all listings that have products with specified sku
				var listings = await GetListingsBySku( shop, sku ).ConfigureAwait( false );
				var listing = listings.FirstOrDefault();

				if (listing != null)
				{
					// get listing's product
					listingProduct = await GetListingProductBySku( listing, sku ).ConfigureAwait( false );
				}
			}

			return listingProduct;
		}

		/// <summary>
		///	Returns shop's active listings
		/// </summary>
		/// <param name="shop">Shop</param>
		/// <param name="sku">Product sku</param>
		/// <returns></returns>
		public async Task < IEnumerable< Listing > > GetListingsBySku( Shop shop, string sku )
		{
			var mark = Mark.CreateNew();
			string url = String.Format( GetShopActiveListingsUrl, shop.Id );

			try
			{
				EtsyLogger.LogStarted( this.CreateMethodCallInfo( url, mark, additionalInfo : this.AdditionalLogInfo() ) );

				var listings = await GetEntitiesAsync< Listing >( url, mark: mark ).ConfigureAwait( false );

				EtsyLogger.LogEnd( this.CreateMethodCallInfo( url, mark, methodResult: listings.ToJson(), additionalInfo : this.AdditionalLogInfo() ) );

				return listings
					.Where( listing => listing.Sku.Select( item => item.ToLower() ).Contains( sku.ToLower() ) )
					.ToArray();
			}
			catch (Exception exception)
			{
				var etsyException = new EtsyException( this.CreateMethodCallInfo( url, mark, additionalInfo : this.AdditionalLogInfo() ), exception );
				EtsyLogger.LogTraceException( etsyException );
				throw etsyException;
			}
		}

		/// <summary>
		///	Returns listing's products
		/// </summary>
		/// <param name="listing"></param>
		/// <param name="sku"></param>
		/// <returns></returns>
		public async Task< ListingProduct > GetListingProductBySku( Listing listing, string sku )
		{
			var mark = Mark.CreateNew();
			string url = String.Format( GetListingProductsInventoryUrl, listing.Id );

			try
			{
				var inventory = await GetEntityAsync< ListingInventory >( url, mark ).ConfigureAwait( false );

				return inventory.Products
					.FirstOrDefault( product => product.Sku != null && product.Sku.ToLower().Equals( sku.ToLower() ) );
			}
			catch ( Exception exception )
			{
				var etsyException = new EtsyException( this.CreateMethodCallInfo( url, mark, additionalInfo : this.AdditionalLogInfo() ), exception );
				EtsyLogger.LogTraceException( etsyException );
				throw etsyException;
			}
		}
	}
}
