using EtsyAccess.Services.Items;
using EtsyAccess.Services.Orders;
using System;
using System.Collections.Generic;
using System.Text;
using EtsyAccess.Models.Configuration;
using EtsyAccess.Models.Throttling;
using EtsyAccess.Services.Authentication;
using EtsyAccess.Services.Common;

namespace EtsyAccess
{
	public interface IEtsyServicesFactory
	{
		IEtsyAuthenticationService CreateAuthenticationService( EtsyConfig config, Throttler throttler );
		IEtsyOrdersService CreateOrdersService( EtsyConfig config, Throttler throttler );
		IEtsyItemsService CreateItemsService( EtsyConfig config, Throttler throttler );
		IEtsyAdminService CreateAdminService( EtsyConfig config, Throttler throttler );
	}
}
