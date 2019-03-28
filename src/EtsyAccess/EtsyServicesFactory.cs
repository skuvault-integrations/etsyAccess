using System;
using CuttingEdge.Conditions;
using EtsyAccess.Models.Configuration;
using EtsyAccess.Models.Throttling;
using EtsyAccess.Services.Authentication;
using EtsyAccess.Services.Common;
using EtsyAccess.Services.Items;
using EtsyAccess.Services.Orders;

namespace EtsyAccess
{
	public class EtsyServicesFactory : IEtsyServicesFactory
	{
		private readonly EtsyConfig _config;
		private readonly Throttler _throttler;

		public EtsyServicesFactory( EtsyConfig config )
		{
			Condition.Requires( config ).IsNotNull();

			_config = config;
			_throttler = new Throttler( _config.ThrottlingMaxRequestsPerRestoreInterval, _config.ThrottlingRestorePeriodInSeconds, _config.ThrottlingMaxRetryAttempts );
		}

		/// <summary>
		///	Returns service to work with Etsy's listings and products
		/// </summary>
		/// <returns></returns>
		public IEtsyItemsService CreateItemsService()
		{
			return new EtsyItemsService( _config, _throttler );
		}

		/// <summary>
		///	Returns service to work with Etsy's general functionality
		/// </summary>
		/// <returns></returns>
		public IEtsyAdminService CreateAdminService()
		{
			return new EtsyAdminService( _config, _throttler );
		}

		/// <summary>
		///	Returns service to work with credentials
		/// </summary>
		/// <returns></returns>
		public IEtsyAuthenticationService CreateAuthenticationService()
		{
			return new EtsyAuthenticationService( _config, _throttler );
		}

		/// <summary>
		///	Returns service to work with Etsy's receipts
		/// </summary>
		/// <returns></returns>
		public IEtsyOrdersService CreateOrdersService()
		{
			return new EtsyOrdersService( _config, _throttler );
		}
	}
}
