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
				.GetPermanentCredentials( "4e584a33de044fef2ace181bbaa27f", "221cdd56ff", "3df7de6a").GetAwaiter().GetResult();

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
