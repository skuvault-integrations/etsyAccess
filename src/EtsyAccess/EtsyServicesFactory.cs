using System;
using EtsyAccess.Services.Authentication;
using EtsyAccess.Services.Items;
using EtsyAccess.Services.Orders;

namespace EtsyAccess
{
	public class EtsyServicesFactory : IEtsyServicesFactory
	{
		private readonly string _applicationKey;
		private readonly string _sharedSecret;

		public EtsyServicesFactory(string applicationKey, string sharedSecret)
		{
			_applicationKey = applicationKey;
			_sharedSecret = sharedSecret;
		}

		public IItemsService CreateItemsService()
		{
			throw new NotImplementedException();
		}

		public IAuthenticationService CreateAuthenticationService()
		{
			return new AuthenticationService( _applicationKey, _sharedSecret );
		}

		public IOrdersService CreateOrdersService( string token, string tokenSecret )
		{
			return new OrdersService( _applicationKey, _sharedSecret, token, tokenSecret);
		}
	}
}
