using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EtsyAccess.Misc
{
	public static class Misc
	{
		public static DateTime FromEpochTime(this long epochTime)
		{
			return new DateTime(1970, 1, 1).AddSeconds(epochTime);
		}

		public static int FromUtcTimeToEpoch(this DateTime date)
		{
			return ( int )( date - new DateTime( 1970, 1, 1 ) ).TotalSeconds;
		}

		public static string ToJson( this object source )
		{
			try
			{
				if (source == null)
					return "{}";
				else
				{
					var serialized = JsonConvert.SerializeObject(source, new IsoDateTimeConverter());
					return serialized;
				}
			}
			catch( Exception )
			{
				return "{}";
			}
		}
	}
}
