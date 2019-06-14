using EtsyAccess;
using EtsyAccess.Exceptions;
using EtsyAccess.Models.Configuration;
using EtsyAccess.Models.Throttling;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EtsyAccessTests
{
	[ TestFixture ]
	public class CancellationTokenTests : BaseTest
	{
		[ Test ]
		public void CancelRequest()
		{
			var cancellationTokenSource = new CancellationTokenSource();
			var sku = "TestSku";

			Assert.ThrowsAsync< EtsyException >( async () =>
			{
				cancellationTokenSource.Cancel();
				await this.EtsyItemsService.GetListingProductBySku( sku, cancellationTokenSource.Token );
				Assert.Fail();
			}, "Task was cancelled" );
		}

		[ Test ]
		public void RequestTimesOut()
		{
			var requestTimeout = 1;
			const string message = "TaskCanceledException";
			var credentials = LoadCredentials();
			var config = new EtsyConfig( credentials.ShopName, credentials.Token, credentials.TokenSecret, requestTimeout );

			var factory = new EtsyServicesFactory( credentials.ApplicationKey, credentials.SharedSecret );
			var throttler = new Throttler( config.ThrottlingMaxRequestsPerRestoreInterval, config.ThrottlingRestorePeriodInSeconds, config.ThrottlingMaxRetryAttempts );

			EtsyOrdersService = factory.CreateOrdersService( config, throttler );
			var cancellationTokenSource = new CancellationTokenSource();
			
			var etsyException = Assert.Throws< EtsyException >( () =>
			{
				base.EtsyOrdersService.GetOrders( DateTime.Now.AddMonths( -3 ), DateTime.Now, cancellationTokenSource.Token );
			});

			Assert.IsNotNull( etsyException );
			Assert.That( etsyException.ToString().Contains( message ) );
		}
	}
}
