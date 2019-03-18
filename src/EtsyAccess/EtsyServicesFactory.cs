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

		public EtsyServicesFactory( string applicationKey, string sharedSecret )
		{
			_applicationKey = applicationKey;
			_sharedSecret = sharedSecret;
		}

		/// <summary>
		///	Returns service to work with Etsy's listings and products
		/// </summary>
		/// <param name="shopId"></param>
		/// <param name="token"></param>
		/// <param name="tokenSecret"></param>
		/// <returns></returns>
		public IItemsService CreateItemsService( int shopId, string token, string tokenSecret )
		{
			return new ItemsService( _applicationKey, _sharedSecret, token, tokenSecret, shopId );
		}

		/// <summary>
		///	Returns service to work with credentials
		/// </summary>
		/// <returns></returns>
		public IAuthenticationService CreateAuthenticationService()
		{
			return new AuthenticationService( _applicationKey, _sharedSecret );
		}

		/// <summary>
		///	Returns service to work with Etsy's receipts
		/// </summary>
		/// <param name="shopId"></param>
		/// <param name="token"></param>
		/// <param name="tokenSecret"></param>
		/// <returns></returns>
		public IOrdersService CreateOrdersService( int shopId, string token, string tokenSecret )
		{
			return new OrdersService( _applicationKey, _sharedSecret, token, tokenSecret, shopId );
		}
	}
}
