using System;
using System.Collections.Generic;
using System.Text;

namespace EtsyAccess.Misc
{
	public static class Misc
	{
		public static long GetUnixEpochTime()
		{
			return (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
		}

		public static DateTime FromEpochTime(this long epochTime)
		{
			return new DateTime(1970, 1, 1).AddSeconds(epochTime);
		}
	}
}
