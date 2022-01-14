using System;
using System.IO;
using System.Reflection;
using System.Threading;
using EtsyAccess;
using EtsyAccess.Models.Configuration;
using EtsyAccess.Models.Throttling;
using EtsyAccess.Services.Authentication;
using EtsyAccess.Services.Common;
using EtsyAccess.Services.Items;
using EtsyAccess.Services.Orders;
using NUnit.Framework;

namespace EtsyAccessTests
{
	public class TestCredentials
	{
		public string ShopName { get; set; }
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
		protected CancellationTokenSource CancellationTokenSource;

		[ SetUp ]
		public void Init()
		{
			var credentials = LoadCredentials();

			ShopName = credentials.ShopName;
			var config = new EtsyConfig( credentials.ShopName, credentials.Token, credentials.TokenSecret );

			var factory = new EtsyServicesFactory( credentials.ApplicationKey, credentials.SharedSecret );
			var throttler = new Throttler( config.ThrottlingMaxRequestsPerRestoreInterval, config.ThrottlingRestorePeriodInSeconds, config.ThrottlingMaxRetryAttempts );

			EtsyOrdersService = factory.CreateOrdersService( config, throttler );
			EtsyItemsService = factory.CreateItemsService( config, throttler );
			EtsyAuthenticationService = factory.CreateAuthenticationService( config );
			EtsyAdminService = factory.CreateAdminService( config, throttler );
			CancellationTokenSource = new CancellationTokenSource();
		}

		protected TestCredentials LoadCredentials()
		{
			string path = new Uri( Path.GetDirectoryName( Assembly.GetExecutingAssembly().CodeBase ) ).LocalPath;

			using( var reader = new StreamReader( path + @"\..\..\files\credentials.csv" ) )
			{
				return new TestCredentials()
				{
					ShopName = reader.ReadLine(),
					ApplicationKey = reader.ReadLine(),
					SharedSecret = reader.ReadLine(),
					Token = reader.ReadLine(),
					TokenSecret = reader.ReadLine()
				};
			}
		}
	}
}