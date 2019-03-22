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
		IEtsyAuthenticationService CreateAuthenticationService();
		IEtsyOrdersService CreateOrdersService();
		IEtsyItemsService CreateItemsService();
	}
}
