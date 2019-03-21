using System;
using System.IO;
using System.Reflection;
using System.Security.Policy;
using EtsyAccess;
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
		protected IOrdersService OrdersService { get; set; }
		protected IItemsService ItemsService { get; set; }
		protected IAuthenticationService AuthenticationService { get; set; }
		protected TestCredentials Credentials { get; set; }

		[ SetUp ]
		public void Init()
		{
			Credentials = LoadCredentials();

			var factory = new EtsyServicesFactory( Credentials.ApplicationKey, Credentials.SharedSecret );

			OrdersService = factory.CreateOrdersService( Credentials.ShopId, Credentials.Token, Credentials.TokenSecret );
			ItemsService = factory.CreateItemsService( Credentials.ShopId, Credentials.Token, Credentials.TokenSecret );
			AuthenticationService = factory.CreateAuthenticationService();
		}

		private TestCredentials LoadCredentials()
		{
			string path = new Uri( Path.GetDirectoryName( Assembly.GetExecutingAssembly().CodeBase ) ).LocalPath;

			using( var reader = new StreamReader( path + @"\..\..\credentials.csv" ) )
			{
				return new TestCredentials
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
			var baseService = new BaseService( Credentials.ApplicationKey, Credentials.SharedSecret, Credentials.Token, Credentials.TokenSecret, null );
			var shop = baseService.GetShopInfo( Credentials.ShopName ).GetAwaiter().GetResult();

			shop.Should().NotBeNull();
		}
	}
}