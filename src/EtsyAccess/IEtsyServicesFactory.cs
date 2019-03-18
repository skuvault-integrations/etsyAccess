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
		IOrdersService CreateOrdersService( int shopId, string token, string tokenSecret );
		IItemsService CreateItemsService( int shopId, string token, string tokenSecret );
	}
}
