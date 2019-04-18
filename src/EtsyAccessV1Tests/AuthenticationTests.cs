using FluentAssertions;
using NUnit.Framework;

namespace EtsyAccessV1Tests
{
	public class AuthenticationTests : BaseTest
	{
		[ Test ]
		public void GetPermanentCredentials()
		{
			var credentials = this.EtsyAuthenticationService
				.GetPermanentCredentials( "73af3b94e2be5683551b4ef82f665c", "504a4a7f2d", "6ee6ea1f").GetAwaiter().GetResult();

			credentials.Should().NotBeNull();
		}

		[ Test ]
		public void GetTemporaryCredentials()
		{
			var credentials = this.EtsyAuthenticationService
				.GetTemporaryCredentials( new[] { "listings_w listings_r transactions_r" } ).GetAwaiter().GetResult();

			credentials.Should().NotBeNull();
		}
	}
}
