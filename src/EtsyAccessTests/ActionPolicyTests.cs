using NUnit.Framework;
using EtsyAccess.Models.Throttling;

namespace EtsyAccessTests
{
	public class ActionPolicyTests
	{
		[ Test ]
		public void WhenDelayBeforeNextAttemptIsCalled_ThenItReturnsCorrectDelay( [ Values( 1, 3, 9 )] int retryCount )
		{
			var delay = ActionPolicy.GetDelayBeforeNextAttempt( retryCount );

			Assert.AreEqual( 5 + 20 * retryCount, delay );
		}

	}
}
