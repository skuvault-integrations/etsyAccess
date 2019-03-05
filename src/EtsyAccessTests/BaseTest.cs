using System;
using EtsyAccess;
using EtsyAccess.Services.Items;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EtsyAccess.Services.Orders;

namespace EtsyAccessTests
{
	[TestClass]
	public class BaseTest
	{
		private const string ApplicationKey = "hmmvy1sp7fqfz43d4z6c117l";
		private const string SharedSecret = "airddqdp3t";

		protected IOrdersService OrdersService { get; set; }
		protected IItemsService ItemsService { get; set; }

		[TestMethod]
		public void Init()
		{
			var factory = new EtsyServicesFactory( ApplicationKey, SharedSecret );

			OrdersService = factory.CreateOrdersService();
		}
	}
}