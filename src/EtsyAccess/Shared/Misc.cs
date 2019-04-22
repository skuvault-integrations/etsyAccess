using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EtsyAccess.Shared
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

		/// <summary>
		///	Parses url query string into dictionary
		/// </summary>
		/// <param name="queryParams">Query parameters</param>
		/// <returns></returns>
		public static Dictionary< string, string > ParseQueryParams( string queryParams )
		{
			var result = new Dictionary< string, string >();

			if ( !string.IsNullOrEmpty( queryParams ) )
			{
				string[] keyValuePairs = queryParams.Replace( "?", "" ).Split( '&' );

				foreach ( string keyValuePair in keyValuePairs )
				{
					string[] keyValue = keyValuePair.Split( '=' );

					if ( keyValue.Length == 2 )
					{
						if ( !result.TryGetValue( keyValue[0], out var tmp ) )
							result.Add( keyValue[0], keyValue[1] );
					}
				}
			}

			return result;
		}
	}
}
