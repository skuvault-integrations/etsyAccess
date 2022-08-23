using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CuttingEdge.Conditions;
using EtsyAccess.Exceptions;
using EtsyAccess.Shared;
using EtsyAccess.Models;
using EtsyAccess.Models.Configuration;
using EtsyAccess.Models.Requests;
using EtsyAccess.Models.Throttling;
using Newtonsoft.Json;

namespace EtsyAccess.Services.Items
{
	public class EtsyItemsService : BaseService, IEtsyItemsService
	{
		public EtsyItemsService( string applicationKey, string sharedSecret, EtsyConfig config, Throttler throttler ) 
			: base( applicationKey, sharedSecret, config, throttler )
		{ }

		public void UpdateSkuQuantity( string sku, int quantity, CancellationToken token )
		{
			UpdateSkuQuantityAsync( sku, quantity, token ).GetAwaiter().GetResult();
		}

		/// <summary>
		///	Updates sku's quantity
		/// </summary>
		/// <param name="sku"></param>
		/// <param name="quantity"></param>
		/// <param name="token"></param>
		/// <returns></returns>
		public async Task UpdateSkuQuantityAsync( string sku, int quantity, CancellationToken token )
		{
			Condition.Requires( sku ).IsNotNullOrEmpty();
			Condition.Requires( quantity ).IsGreaterOrEqual( 0 );

			var mark = Mark.CreateNew();

			// get all listings that have products with specified sku
			var listings = await GetListingsBySku( sku, token ).ConfigureAwait( false );
			var listing = listings.FirstOrDefault();

			if ( listing == null)
				return;

			// get listing inventory
			var listingInventory = await GetListingInventoryAsync( listing, token ).ConfigureAwait( false );
						
			await UpdateSkuQuantityAsync( listing, listingInventory, sku, quantity, token ).ConfigureAwait( false );
		}

		/// <summary>
		///	Updates sku quantity asynchronously
		/// </summary>
		/// <param name="listing"></param>
		/// <param name="inventory"></param>
		/// <param name="sku"></param>
		/// <param name="incomingQuantity"></param>
		/// <returns></returns>
		private async Task UpdateSkuQuantityAsync( Listing listing, ListingInventory inventory, string sku, int incomingQuantity, CancellationToken token, Mark mark = null )
		{
			if ( mark == null )
				mark = Mark.CreateNew();

			var updateInventoryRequests = CreateUpdateInventoryRequests( sku, incomingQuantity, inventory.Products ).ToList();

			if ( !updateInventoryRequests.Any() )
				return;

			var url = String.Format( EtsyEndPoint.UpdateListingInventoryUrl, listing.Id );

			Dictionary<string, string> payload;
			try
			{
				EtsyLogger.LogStarted( this.CreateMethodCallInfo( url, mark, additionalInfo : this.AdditionalLogInfo() ) );

				payload = new Dictionary<string, string>
				{
					{ "products", JsonConvert.SerializeObject( updateInventoryRequests.ToArray() ) }
				};

				if (inventory.PriceOnProperty.Length != 0)
					payload.Add("price_on_property", string.Join( ",", inventory.PriceOnProperty ) );

				if (inventory.QuantityOnProperty.Length != 0)
					payload.Add("quantity_on_property", string.Join( ",", inventory.QuantityOnProperty ) );

				if (inventory.SkuOnProperty.Length != 0)
					payload.Add("sku_on_property", string.Join( ",", inventory.SkuOnProperty ) );
			}
			catch ( Exception exception )
			{
				var etsyException = new EtsyException( this.CreateMethodCallInfo( url, mark, additionalInfo : this.AdditionalLogInfo() ), exception );
				EtsyLogger.LogTraceException( etsyException );
				throw etsyException;
			}

			var requestPayload = payload.ToJson();
			try
			{
				await base.PutAsync( url, payload, token, mark ).ConfigureAwait( false );

				EtsyLogger.LogEnd( this.CreateMethodCallInfo( url, mark, additionalInfo : this.AdditionalLogInfo(), requestPayload: requestPayload ) );
			}
			catch ( Exception exception )
			{
				var etsyException = new EtsyException( this.CreateMethodCallInfo( url, mark, additionalInfo : this.AdditionalLogInfo(), requestPayload: requestPayload ), exception );
				EtsyLogger.LogTraceException( etsyException );
				throw etsyException;
			}
		}

		private static PropertyValue[] DecodePropertyValuesWithQuotesAndEscape( PropertyValue[] properties )
		{
			if ( properties == null && !properties.Any() )
				return properties;

			return properties.Select( p => p.DecodeValuesQuotesAndEscape() ).ToArray();
		}
		
		public static IEnumerable< UpdateInventoryRequest > CreateUpdateInventoryRequests( string sku, int incomingQuantity, ListingProduct[] products )
		{
			var requests = new List< UpdateInventoryRequest >();
			var hasQuantityChanged = false;

			//Etsy requires we send ALL product variations, including the ones with unchanged quantity
			foreach ( var product in products )
			{
				var productOffering = product.Offerings.FirstOrDefault();

				if ( productOffering == null )
					continue;

				var productQuantity = productOffering.Quantity;

				if ( product.Sku != null && product.Sku.ToLower().Equals( sku.ToLower() ) )
				{
					if ( productQuantity != incomingQuantity )
						hasQuantityChanged = true;
					productQuantity = incomingQuantity;
				}

				requests.Add( new UpdateInventoryRequest
				{
					ProductId = product.Id,
					Sku = product.Sku,
					PropertyValues = DecodePropertyValuesWithQuotesAndEscape( product.PropertyValues ),
					// currently each product has one offering
					ListingOffering = new []
					{
						new ListingOfferingRequest
						{
							Id = productOffering.Id,
							Quantity = productQuantity,
							Price = ( decimal ) productOffering.Price
						}
					}
				});
			}

			return hasQuantityChanged ? requests : new List< UpdateInventoryRequest >();
		}

		/// <summary>
		///	Updates skus quantities
		/// </summary>
		/// <param name="skusQuantities"></param>
		/// <param name="token"></param>
		/// <returns></returns>
		public async Task UpdateSkusQuantityAsync( Dictionary< string, int > skusQuantities, CancellationToken token, Mark mark )
		{
			Condition.Requires( skusQuantities ).IsNotEmpty();

			var listings = await GetListingsBySkus( skusQuantities.Keys, token, mark ).ConfigureAwait( false );

			foreach ( var skuQuantity in skusQuantities )
			{
				var sku = skuQuantity.Key;
				var quantity = skuQuantity.Value;

				var listing = listings.Where( item => item.Sku.Select( s => s.ToLower() ).Contains( sku.ToLower() ) ).FirstOrDefault();

				if ( listing == null )
					continue;

				var listingInventory = await GetListingInventoryAsync( listing, token, mark ).ConfigureAwait( false );

				if ( listingInventory != null )
					await UpdateSkuQuantityAsync( listing, listingInventory, sku, quantity, token, mark ).ConfigureAwait( false );
			}
		}

		/// <summary>
		///	Returns listings' product by sku
		/// </summary>
		/// <param name="sku"></param>
		/// <param name="token"></param>
		/// <returns></returns>
		public async Task< ListingProduct > GetListingProductBySku( string sku, CancellationToken token )
		{
			Condition.Requires( sku ).IsNotNullOrEmpty();

			ListingProduct listingProduct = null;

			var inventory = await GetListingInventoryBySku( sku, token ).ConfigureAwait( false );

			if ( inventory != null )
				listingProduct = inventory.Products
								.FirstOrDefault( product => product.Sku != null && product.Sku.ToLower().Equals( sku.ToLower() ) );

			return listingProduct;
		}

		/// <summary>
		///	Returns sku inventory
		/// </summary>
		/// <param name="sku"></param>
		/// <param name="token"></param>
		/// <returns></returns>
		public async Task< ListingInventory > GetListingInventoryBySku( string sku, CancellationToken token )
		{
			Condition.Requires( sku ).IsNotNullOrEmpty();

			ListingInventory listingInventory = null;

			// get all listings that have products with specified sku
			var listings = await GetListingsBySku( sku, token ).ConfigureAwait( false );
			var listing = listings.FirstOrDefault();
			
			// get listing's product inventory
			if ( listing != null )
				listingInventory = await GetListingInventoryAsync( listing, token ).ConfigureAwait( false );

			return listingInventory;
		}


		///  <summary>
		/// 	Returns listing's products
		///  </summary>
		///  <param name="listing"></param>
		///  <param name="token"></param>
		///  <param name="mark"></param>
		///  <returns></returns>
		public async Task< ListingInventory > GetListingInventoryAsync( Listing listing, CancellationToken token, Mark mark = null )
		{
			if ( mark == null )
				mark = Mark.CreateNew();

			string url = String.Format( EtsyEndPoint.GetListingInventoryUrl, listing.Id );

			try
			{
				EtsyLogger.LogStarted( this.CreateMethodCallInfo( url, mark, additionalInfo : this.AdditionalLogInfo() ) );

				var inventory = await GetEntityAsync< ListingInventory >( url, token, mark ).ConfigureAwait( false );

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
		/// <param name="token">Product sku</param>
		/// <returns></returns>
		public Task < IEnumerable< Listing > > GetListingsBySku( string sku, CancellationToken token )
		{
			return GetListingsBySkus( new string[] { sku }, token );
		}

		///  <summary>
		/// 	Returns listings with specified skus
		///  </summary>
		///  <param name="skus"></param>
		///  <param name="token"></param>
		///  <param name="mark"></param>
		///  <returns></returns>
		public async Task< IEnumerable< Listing > > GetListingsBySkus( IEnumerable< string > skus, CancellationToken token, Mark mark = null )
		{
			if ( mark == null )
				mark = Mark.CreateNew();

			var url = String.Format( EtsyEndPoint.GetShopActiveListingsUrl, Config.ShopName );

			try
			{
				EtsyLogger.LogStarted( this.CreateMethodCallInfo( url, mark, additionalInfo : this.AdditionalLogInfo() ) );

				var listings = await GetEntitiesAsync< Listing >( url, token, mark: mark ).ConfigureAwait( false );

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

		/// <summary>
		///	Returns listings in the draft state
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		public async Task< IEnumerable< Listing > > GetDraftListings( CancellationToken token )
		{
			var mark = Mark.CreateNew();
			var url = String.Format( EtsyEndPoint.GetShopDraftListingsUrl, Config.ShopName );

			try
			{
				EtsyLogger.LogStarted( this.CreateMethodCallInfo( url, mark, additionalInfo : this.AdditionalLogInfo() ) );

				var listings = await GetEntitiesAsync< Listing >( url, token, mark: mark ).ConfigureAwait( false );

				EtsyLogger.LogEnd( this.CreateMethodCallInfo( url, mark, methodResult: listings.ToJson(), additionalInfo : this.AdditionalLogInfo() ) );

				return listings.ToArray();

			}
			catch (Exception exception)
			{
				var etsyException = new EtsyException( this.CreateMethodCallInfo( url, mark, additionalInfo : this.AdditionalLogInfo() ), exception );
				EtsyLogger.LogTraceException( etsyException );
				throw etsyException;
			}
		}

		/// <summary>
		///	Creates a new listing
		/// </summary>
		/// <param name="request"></param>
		/// <param name="token"></param>
		/// <returns></returns>
		public async Task CreateListing( PostListingRequest request, CancellationToken token )
		{
			var mark = Mark.CreateNew();
			var url = String.Format( EtsyEndPoint.CreateListingUrl, Config.ShopName );

			try
			{
				EtsyLogger.LogStarted( this.CreateMethodCallInfo( url, mark, additionalInfo : this.AdditionalLogInfo() ) );

				var payload = new Dictionary< string, string >
				{
					{ "quantity", request.Quantity.ToString() },
					{ "title", request.Title },
					{ "description", request.Description },
					{ "price", request.Price.ToString() },
					{ "who_made", request.WhoMade.ToString() },
					{ "is_supply", request.IsSupply.ToString() },
					{ "when_made", request.WhenMade },
					{ "state", request.State.ToString() },
					{ "shipping_template_id", request.ShippingTemplateId.ToString() }
				};

				await base.PostAsync( url, payload, token, mark ).ConfigureAwait( false );

				EtsyLogger.LogEnd( this.CreateMethodCallInfo( url, mark, methodResult: request.ToJson(), additionalInfo : this.AdditionalLogInfo() ) );

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
