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
		private readonly string _shopName;

		public EtsyServicesFactory(string applicationKey, string sharedSecret, string shopName )
		{
			_applicationKey = applicationKey;
			_sharedSecret = sharedSecret;

			_shopName = shopName;
		}

		public IItemsService CreateItemsService( string token, string tokenSecret )
		{
			return new ItemsService( _shopName, _applicationKey, _sharedSecret, token, tokenSecret );
		}

		public IAuthenticationService CreateAuthenticationService()
		{
			return new AuthenticationService( _applicationKey, _sharedSecret );
		}

		public IOrdersService CreateOrdersService( string token, string tokenSecret )
		{
			return new OrdersService( _shopName, _applicationKey, _sharedSecret, token, tokenSecret );
		}
	}
}
