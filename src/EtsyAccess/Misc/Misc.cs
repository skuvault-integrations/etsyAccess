using System;
using System.Collections.Generic;
using System.Text;

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
	}
}
