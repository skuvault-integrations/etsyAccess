using System;
using System.IO;
using System.Reflection;
using System.Security.Policy;
using EtsyAccess;
using EtsyAccess.Models.Configuration;
using EtsyAccess.Models.Throttling;
using EtsyAccess.Services;
using EtsyAccess.Services.Authentication;
using EtsyAccess.Services.Common;
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
		protected IEtsyAdminService EtsyAdminService { get; set; }
		protected string ShopName;
		protected EtsyConfig Config;

		[ SetUp ]
		public void Init()
		{
			var credentials = LoadCredentials();

			ShopName = credentials.ShopName;
			var config = new EtsyConfig( credentials.ApplicationKey, credentials.SharedSecret, credentials.ShopId,
				credentials.Token, credentials.TokenSecret );

			var factory = new EtsyServicesFactory();
			var throttler = new Throttler( config.ThrottlingMaxRequestsPerRestoreInterval, config.ThrottlingRestorePeriodInSeconds, config.ThrottlingMaxRetryAttempts );

			EtsyOrdersService = factory.CreateOrdersService( config, throttler );
			EtsyItemsService = factory.CreateItemsService( config, throttler );
			EtsyAuthenticationService = factory.CreateAuthenticationService( config, throttler );
			EtsyAdminService = factory.CreateAdminService( config, throttler);
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
	}
}