using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace EtsyAccess.Models
{
	/// <summary>
	/// A representation of a listing's inventory
	/// </summary>
	public class ListingInventory
	{
		/// <summary>
		/// The products available for this listing
		/// </summary>
		[JsonProperty("products")]
		public ListingProduct[] Products { get; set; }
		/// <summary>
		/// Which properties control price?
		/// </summary>
		[JsonProperty("price_on_property")]
		public int[] PriceOnProperty { get; set; }
		/// <summary>
		/// Which properties control quantity?
		/// </summary>
		[JsonProperty("quantity_on_property")]
		public int[] QuantityOnProperty { get; set; }
		/// <summary>
		/// Which properties control SKU?
		/// </summary>
		[JsonProperty("sku_on_property")]
		public int[] SkuOnProperty { get; set; }
	}
}
