using EtsyAccess.Services.Items;
using EtsyAccess.Services.Orders;
using EtsyAccess.Models.Configuration;
using EtsyAccess.Models.Throttling;
using EtsyAccess.Services.Authentication;
using EtsyAccess.Services.Common;

namespace EtsyAccess
{
	public interface IEtsyServicesFactory
	{
		IEtsyAuthenticationService CreateAuthenticationService( EtsyConfig config );
		IEtsyOrdersService CreateOrdersService( EtsyConfig config, Throttler throttler );
		IEtsyItemsService CreateItemsService( EtsyConfig config, Throttler throttler );
		IEtsyAdminService CreateAdminService( EtsyConfig config, Throttler throttler );
	}
}
