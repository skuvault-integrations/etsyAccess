using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EtsyAccess.Services;
using FluentAssertions;
using NUnit.Framework;

namespace EtsyAccessV1Tests
{
	public class AdminTest : BaseTest
	{
		[ Test ]
		public void GetShopInfoByName()
		{
			var shop = EtsyAdminService.GetShopInfo( ShopName, CancellationToken.None ).GetAwaiter().GetResult();

			shop.Should().NotBeNull();
		}
	}
}
