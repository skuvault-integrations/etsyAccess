using NUnit.Framework;
using EtsyAccess.Models.Throttling;

namespace EtsyAccessTests
{
	public class ActionPolicyTests
	{
		[ Test ]
		public void GetDelayBeforeNextAttempt( [ Values( 1, 3, 9 )] int retryCount )
		{
			var delay = ActionPolicy.GetDelayBeforeNextAttempt( retryCount );

			Assert.AreEqual( 5 + 20 * retryCount, delay );
		}

	}
}
