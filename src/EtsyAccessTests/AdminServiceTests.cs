using System.Threading;
using FluentAssertions;
using NUnit.Framework;

namespace EtsyAccessTests
{
	public class AdminServiceTests : BaseTest
	{
		[ Test ]
		public void GetShopInfoByName()
		{
			var shop = EtsyAdminService.GetShopInfo( ShopName, CancellationToken.None ).GetAwaiter().GetResult();

			shop.Should().NotBeNull();
		}
	}
}
