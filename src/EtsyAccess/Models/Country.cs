using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace EtsyAccess.Models
{
	public class Country
	{
		[JsonProperty("country_id")]
		public int Id { get; set; }
		[JsonProperty("iso_country_code")]
		public string IsoCountryCode { get; set; }
		[JsonProperty("world_bank_country_code")]
		public string WorldBankCountryCode { get; set; }
		[JsonProperty("name")]
		public string Name { get; set; }
		[JsonProperty("slug")]
		public string Slug { get; set; }
	}
}
