using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace EtsyAccess.Models
{
	/// <summary>
	/// A representation of a product for a listing.
	/// </summary>
	public class ListingProduct
	{
		/// <summary>
		///	The numeric ID of this product
		/// </summary>
		[JsonProperty("product_id")]
		public long Id { get; set; }
		/// <summary>
		/// The product identifier (if set).
		/// </summary>
		[JsonProperty("sku")]
		public string Sku { get; set; }
		/// <summary>
		/// A list of 0-2 properties associated with this product and their values.
		/// </summary>
		[JsonProperty("property_values")]
		public string[] PropertyValues { get; set; }
		/// <summary>
		/// Has the product been deleted?
		/// </summary>
		[JsonProperty("is_deleted")]
		public bool IsDeleted { get; set; }
		/// <summary>
		/// List of active offerings for this product
		/// </summary>
		public ListingOffering[] Offerings { get; set; }
	}
}
