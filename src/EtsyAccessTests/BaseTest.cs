using System;
using EtsyAccess;
using EtsyAccess.Services;
using EtsyAccess.Services.Authentication;
using EtsyAccess.Services.Items;
using EtsyAccess.Services.Orders;
using FluentAssertions;
using NUnit.Framework;

namespace EtsyAccessTests
{
	public class BaseTest
	{
		protected const string ShopName = "SkuVaultInc";
		protected const int ShopId = 19641381;

		private const string ApplicationKey = "hmmvy1sp7fqfz43d4z6c117l";
		private const string SharedSecret = "airddqdp3t";

		private const string Token = "3168607b820c492e24d0b2f46abd96";
		private const string TokenSecret = "8a53e2e484";

		protected IOrdersService OrdersService { get; set; }
		protected IItemsService ItemsService { get; set; }
		protected IAuthenticationService AuthenticationService { get; set; }

		[ SetUp ]
		public void Init()
		{
			var factory = new EtsyServicesFactory( ApplicationKey, SharedSecret );

			OrdersService = factory.CreateOrdersService( ShopId, Token, TokenSecret );
			ItemsService = factory.CreateItemsService( ShopId, Token, TokenSecret );
			AuthenticationService = factory.CreateAuthenticationService();
		}

		
		[ Test ]
		public void GetShopInfoByName()
		{
			var baseService = new BaseService(ApplicationKey, SharedSecret, Token, TokenSecret, null);
			var shop = baseService.GetShopInfo(ShopName).GetAwaiter().GetResult();

			shop.Should().NotBeNull();
		}
	}
}