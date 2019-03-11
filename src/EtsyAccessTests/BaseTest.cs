using System;
using EtsyAccess;
using EtsyAccess.Services.Authentication;
using EtsyAccess.Services.Items;
using EtsyAccess.Services.Orders;
using NUnit.Framework;

namespace EtsyAccessTests
{
	public class BaseTest
	{
		private const string ApplicationKey = "hmmvy1sp7fqfz43d4z6c117l";
		private const string SharedSecret = "airddqdp3t";

		protected IOrdersService OrdersService { get; set; }
		protected IItemsService ItemsService { get; set; }
		protected IAuthenticationService AuthenticationService { get; set; }

		[ SetUp ]
		public void Init()
		{
			var factory = new EtsyServicesFactory();

			OrdersService = factory.CreateOrdersService();
			AuthenticationService = factory.CreateAuthenticationService( ApplicationKey, SharedSecret );
		}
	}
}