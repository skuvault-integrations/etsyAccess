using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace EtsyAccessTests
{
	public class AuthenticationTests : BaseTest
	{
		[ Test ]
		public void GetPermanentCredentials()
		{
			var credentials = this.AuthenticationService
				.GetPermanentCredentials( "73af3b94e2be5683551b4ef82f665c", "504a4a7f2d", "6ee6ea1f").GetAwaiter().GetResult();

			credentials.Should().NotBeNull();
		}

		[ Test ]
		public void GetTemporaryCredentials()
		{
			var credentials = this.AuthenticationService
				.GetTemporaryCredentials(new string[] {"listings_w transactions_r"}).GetAwaiter().GetResult();

			credentials.Should().NotBeNull();
		}
	}
}
