using EtsyAccess.Services.Items;
using EtsyAccess.Services.Orders;
using System;
using System.Collections.Generic;
using System.Text;

namespace EtsyAccess
{
	public interface IEtsyServicesFactory
	{
		IOrdersService CreateOrdersService();
		IItemsService CreateItemsService();
	}
}
