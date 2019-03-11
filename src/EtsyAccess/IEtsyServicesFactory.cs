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
		IAuthenticationService CreateAuthenticationService( string applicationKey, string sharedSecret );
		IOrdersService CreateOrdersService();
		IItemsService CreateItemsService();
	}
}
