using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace EtsyAccess.Models
{
	public class PropertyValue
	{
		/// <summary>
		///	The numeric ID of this property
		/// </summary>
		[JsonProperty("property_id")]
		public long Id { get; set; }
		/// <summary>
		/// The name of the property, in the requested locale language
		/// </summary>
		[JsonProperty("property_name")]
		public string Name { get; set; }
		/// <summary>
		///	The numeric ID of the scale (if any)
		/// </summary>
		[JsonProperty("scale_id")]
		public int? ScaleId { get; set; }
		/// <summary>
		///	The label used to describe the chosen scale (if any).
		/// </summary>
		[JsonProperty("scale_name")]
		public string ScaleName { get; set; }
		/// <summary>
		///	The numeric IDs of the values
		/// </summary>
		[JsonProperty("value_ids")]
		public long[] ValueIds { get; set; }
		/// <summary>
		/// The literal values of the value.
		/// </summary>
		[JsonProperty("values")]
		public string[] Values { get; set; }
	}
}
