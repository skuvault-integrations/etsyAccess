using System;
using System.IO;
using System.Reflection;
using System.Security.Policy;
using EtsyAccess;
using EtsyAccess.Models.Configuration;
using EtsyAccess.Services;
using EtsyAccess.Services.Authentication;
using EtsyAccess.Services.Items;
using EtsyAccess.Services.Orders;
using FluentAssertions;
using NUnit.Framework;

namespace EtsyAccessTests
{
	public class TestCredentials
	{
		public string ShopName { get; set; }
		public int ShopId { get; set; }
		public string ApplicationKey { get; set; }
		public string SharedSecret { get; set; }
		public string Token { get; set; }
		public string TokenSecret { get; set; }
	}

	public class BaseTest
	{
		protected IEtsyOrdersService EtsyOrdersService { get; set; }
		protected IEtsyItemsService EtsyItemsService { get; set; }
		protected IEtsyAuthenticationService EtsyAuthenticationService { get; set; }
		protected BaseService BaseService { get; set; }
		protected string ShopName;
		protected EtsyConfig Config;

		[ SetUp ]
		public void Init()
		{
			var credentials = LoadCredentials();

			ShopName = credentials.ShopName;
			var config = new EtsyConfig( credentials.ApplicationKey, credentials.SharedSecret, credentials.ShopId,
				credentials.Token, credentials.TokenSecret );

			var factory = new EtsyServicesFactory( config );

			EtsyOrdersService = factory.CreateOrdersService();
			EtsyItemsService = factory.CreateItemsService();
			EtsyAuthenticationService = factory.CreateAuthenticationService();
			BaseService = new BaseService( config );
		}

		private TestCredentials LoadCredentials()
		{
			string path = new Uri( Path.GetDirectoryName( Assembly.GetExecutingAssembly().CodeBase ) ).LocalPath;

			using( var reader = new StreamReader( path + @"\..\..\credentials.csv" ) )
			{
				return new TestCredentials()
				{
					ShopName = reader.ReadLine(),
					ShopId = int.Parse( reader.ReadLine() ),
					ApplicationKey = reader.ReadLine(),
					SharedSecret = reader.ReadLine(),
					Token = reader.ReadLine(),
					TokenSecret = reader.ReadLine()
				};
			}
		}
		
		[ Test ]
		public void GetShopInfoByName()
		{
			var shop = BaseService.GetShopInfo( ShopName ).GetAwaiter().GetResult();

			shop.Should().NotBeNull();
		}
	}
}