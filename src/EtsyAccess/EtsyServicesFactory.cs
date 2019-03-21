using System;
using CuttingEdge.Conditions;
using EtsyAccess.Models.Configuration;
using EtsyAccess.Services.Authentication;
using EtsyAccess.Services.Items;
using EtsyAccess.Services.Orders;

namespace EtsyAccess
{
	public class EtsyServicesFactory : IEtsyServicesFactory
	{
		private readonly EtsyConfig Config;

		public EtsyServicesFactory( EtsyConfig config )
		{
			Condition.Requires( config ).IsNotNull();

			this.Config = config;
		}

		/// <summary>
		///	Returns service to work with Etsy's listings and products
		/// </summary>
		/// <returns></returns>
		public IItemsService CreateItemsService()
		{
			return new ItemsService( Config );
		}

		/// <summary>
		///	Returns service to work with credentials
		/// </summary>
		/// <returns></returns>
		public IAuthenticationService CreateAuthenticationService()
		{
			return new AuthenticationService( Config );
		}

		/// <summary>
		///	Returns service to work with Etsy's receipts
		/// </summary>
		/// <returns></returns>
		public IOrdersService CreateOrdersService( )
		{
			return new OrdersService( Config );
		}
	}
}
