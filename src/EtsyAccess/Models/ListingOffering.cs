using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace EtsyAccess.Models
{
	/// <summary>
	/// A representation of an offering for a listing.
	/// </summary>
	public class ListingOffering
	{
		/// <summary>
		/// The numeric ID of this offering.
		/// </summary>
		[JsonProperty("offering_id")]
		public long Id { get; set; }
		/// <summary>
		/// The price of the product
		/// </summary>
		[JsonProperty("price")]
		public Money Price { get; set; }
		/// <summary>
		/// How many of this product are available?
		/// </summary>
		[JsonProperty("quantity")]
		public int Quantity { get; set; }
		/// <summary>
		/// Has the offering been deleted?
		/// </summary>
		[JsonProperty("is_enabled")]
		public bool IsEnabled { get; set; }
		/// <summary>
		/// Is the offering shown to buyers?
		/// </summary>
		[JsonProperty("is_deleted")]
		public bool IsDeleted { get; set; }
	}
}
