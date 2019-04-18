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
		private readonly string _applicationKey;
		private readonly string _sharedSecret;
		public EtsyServicesFactory( string applicationKey, string sharedSecret )
		{
			Condition.Requires( applicationKey ).IsNotNullOrEmpty();
			Condition.Requires( sharedSecret ).IsNotNullOrEmpty();

			_applicationKey = applicationKey;
			_sharedSecret = sharedSecret;
		}

		/// <summary>
		///	Returns service to work with Etsy's listings and products
		/// </summary>
		/// <returns></returns>
		public IEtsyItemsService CreateItemsService( EtsyConfig config, Throttler throttler )
		{
			Condition.Requires( config ).IsNotNull();
			Condition.Requires( throttler ).IsNotNull();

			return new EtsyItemsService( this._applicationKey, this._sharedSecret, config, throttler );
		}

		/// <summary>
		///	Returns service to work with Etsy's general functionality
		/// </summary>
		/// <returns></returns>
		public IEtsyAdminService CreateAdminService( EtsyConfig config, Throttler throttler )
		{
			Condition.Requires( config ).IsNotNull();
			Condition.Requires( throttler ).IsNotNull();

			return new EtsyAdminService( this._applicationKey, this._sharedSecret, config, throttler );
		}

		/// <summary>
		///	Returns service to work with credentials
		/// </summary>
		/// <returns></returns>
		public IEtsyAuthenticationService CreateAuthenticationService( EtsyConfig config )
		{
			Condition.Requires( config ).IsNotNull();
			var throttler = new Throttler( config.ThrottlingMaxRequestsPerRestoreInterval, config.ThrottlingRestorePeriodInSeconds, config.ThrottlingMaxRetryAttempts );

			return new EtsyAuthenticationService( this._applicationKey, this._sharedSecret, config, throttler );
		}

		/// <summary>
		///	Returns service to work with Etsy's receipts
		/// </summary>
		/// <returns></returns>
		public IEtsyOrdersService CreateOrdersService( EtsyConfig config, Throttler throttler )
		{
			Condition.Requires( config ).IsNotNull();
			Condition.Requires( throttler ).IsNotNull();

			return new EtsyOrdersService(  this._applicationKey, this._sharedSecret, config, throttler );
		}
	}
}
