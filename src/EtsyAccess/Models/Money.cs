using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace EtsyAccess.Models
{
	/// <summary>
	/// A representation of an amount of money
	/// </summary>
	public class Money
	{
		/// <summary>
		/// The amount of money represented by this data.
		/// </summary>
		[JsonProperty("amount")]
		public int Amount { get; set; }
		/// <summary>
		/// The divisor to render the amount
		/// </summary>
		[JsonProperty("divisor")]
		public int Divisor { get; set; }
		/// <summary>
		/// The requested locale currency
		/// </summary>
		[JsonProperty("currency_code")]
		public string CurrencyCode { get; set; }
		/// <summary>
		/// The formatted amount without codes or symbols in the requested locale's numeric style, e.g. '10.42'
		/// </summary>
		[JsonProperty("formatted_raw")]
		public string FormattedRaw { get; set; }
		/// <summary>
		/// The formatted amount with a symbol in the requested locale's numeric style, e.g. 'US$10.42'.
		/// </summary>
		[JsonProperty("formatted_short")]
		public string FormattedShort { get; set; }
		/// <summary>
		/// The formatted amount with a symbol and currency in the requested locale's numeric style, e.g. '$10.42 USD'.
		/// </summary>
		[JsonProperty("formatted_long")]
		public string FormattedLong { get; set; }
		/// <summary>
		/// The original currency code the value was listed in (if the value has been converted). 
		/// Deprecated: Replaced by "before_conversion" (to be removed 15 February 2017).
		/// </summary>
		[JsonProperty("original_currency_code")]
		public string OriginalCurrencyCode { get; set; }
		/// <summary>
		/// A representation of the value without currency conversion (if conversion has happened).
		/// </summary>
		[JsonProperty("before_conversion")]
		public Money BeforeConversion { get; set; }
	}
}
