using System;
using EtsyAccess.Services.Authentication;
using EtsyAccess.Services.Items;
using EtsyAccess.Services.Orders;

namespace EtsyAccess
{
	public class EtsyServicesFactory : IEtsyServicesFactory
	{
		public IItemsService CreateItemsService()
		{
			throw new NotImplementedException();
		}

		public IAuthenticationService CreateAuthenticationService( string applicationKey, string sharedSecret )
		{
			return new AuthenticationService( applicationKey, sharedSecret );
		}

		public IOrdersService CreateOrdersService()
		{
			return new OrdersService(  "" );
		}
	}
}
