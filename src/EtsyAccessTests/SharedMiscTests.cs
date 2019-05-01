using System;
using EtsyAccess.Shared;
using NUnit.Framework;

namespace EtsyAccessTests
{
	public class SharedMiscTests
	{
		[ Test ]
		public void FromEpochTime()
		{
			const long epochTime = 1556728767;

			var utcTime = epochTime.FromEpochTime();

			Assert.AreEqual( new DateTime( 2019, 5, 1, 16, 39, 27 ), utcTime );
		}

		[ Test ]
		public void FromUtcTimeToEpoch()
		{
			var utcTime = new DateTime( 2019, 5, 1, 16, 41, 27 );

			var epochTime = utcTime.FromUtcTimeToEpoch();

			Assert.AreEqual( 1556728887, epochTime );
		}

		[ Test ]
		public void ParseQueryParams()
		{
			var queryParams = "?what=does&it=&all=mean";

			var parsedParams = Misc.ParseQueryParams(queryParams);

			Assert.AreEqual( 3, parsedParams.Count );
			Assert.AreEqual( "does", parsedParams["what"] );
			Assert.AreEqual( "", parsedParams["it"] );
			Assert.AreEqual( "mean", parsedParams["all"] );
		}
	}
}
