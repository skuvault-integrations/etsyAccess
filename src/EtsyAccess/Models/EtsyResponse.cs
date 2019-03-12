using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace EtsyAccess.Models
{
	public class EtsyPagination
	{
		[JsonProperty("effective_limit")]
		public int EffectiveLimit { get; set; }
		[JsonProperty("effective_offset")]
		public int EffectiveOffset { get; set; }
		[JsonProperty("next_offset")]
		public int? NextOffset { get; set; }
		[JsonProperty("effective_page")]
		public int EffectivePage { get; set; }
		[JsonProperty("next_page")]
		public int? NextPage { get; set; }
	}

	public class EtsyResponse< T >
	{
		public int Count { get; set; }
		public string Type { get; set; }
		public T[] Results { get; set; }
		public EtsyPagination Pagination { get; set; }
	}
}
