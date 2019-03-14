using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace EtsyAccess.Models.Requests
{
	public class UpdateInventoryRequest
	{
		[JsonProperty("product_id")]
		public long ProductId { get; set; }
		[JsonProperty("sku")]
		public string Sku { get; set; }
		[JsonProperty("property_values")]
		public string[] PropertyValues { get; set; }
		[JsonProperty("offerings")]
		public ListingOfferingRequest[] ListingOffering { get; set; }
	}

	public class ListingOfferingRequest
	{
		[JsonProperty("id")]
		public long Id { get; set; }
		[JsonProperty("quantity")]
		public int Quantity { get; set; }
		[JsonProperty("price")]
		public float Price { get; set; }
	}
}
