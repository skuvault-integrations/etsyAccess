using System;
using System.Collections.Generic;
using System.Text;

namespace EtsyAccess.Misc
{
	public class Misc
	{
		public static long GetUnixEpochTime()
		{
			return (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
		}

		public static string EscapeUriDataStringRfc3986(string value) {
			string unreservedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";
			StringBuilder result = new StringBuilder();

			foreach (char symbol in value) {
				if (unreservedChars.IndexOf(symbol) != -1) {
					result.Append(symbol);
				} else {
					result.Append('%' + String.Format("{0:X2}", (int)symbol));
				}
			}

			return result.ToString();
		}
	}
}
