using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace EtsyAccess.Models
{
	public class Shop
	{
		[JsonProperty("shop_id")]
		public int Id { get; set; }
		[JsonProperty("shop_name")]
		public string Name { get; set; }
		[JsonProperty("user_id")]
		public long UserId { get; set; }
		[JsonProperty("creation_tsz")]
		public long CreationTsz { get; set; }
		[JsonProperty("title")]
		public string Title { get; set; }
		[JsonProperty("announcement")]
		public string Announcement { get; set; }
		[JsonProperty("currency_code")]
		public string CurrencyCode { get; set; }
		[JsonProperty("is_vacation")]
		public bool IsVacation { get; set; }
		[JsonProperty("vacation_message")]
		public string VacationMessage { get; set; }
		[JsonProperty("sale_message")]
		public string SaleMessage { get; set; }
		[JsonProperty("digital_sale_message")]
		public string DigitalSaleMessage { get; set; }
		[JsonProperty("last_updated_tsz")]
		public long LastUpdatedTsz { get; set; }
		[JsonProperty("listing_active_count")]
		public long ListingActiveCount { get; set; }
		[JsonProperty("digital_listing_count")]
		public long DigitalListingCount { get; set; }
	}
}
