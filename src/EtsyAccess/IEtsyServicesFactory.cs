using EtsyAccess.Services.Items;
using EtsyAccess.Services.Orders;
using System;
using System.Collections.Generic;
using System.Text;
using EtsyAccess.Services.Authentication;

namespace EtsyAccess
{
	public interface IEtsyServicesFactory
	{
		IAuthenticationService CreateAuthenticationService();
		IOrdersService CreateOrdersService( string token, string tokenSecret );
		IItemsService CreateItemsService();
	}
}
