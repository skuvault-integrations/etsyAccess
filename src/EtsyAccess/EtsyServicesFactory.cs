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
		private readonly EtsyConfig _config;

		public EtsyServicesFactory( EtsyConfig config )
		{
			Condition.Requires( config ).IsNotNull();

			_config = config;
		}

		/// <summary>
		///	Returns service to work with Etsy's listings and products
		/// </summary>
		/// <returns></returns>
		public IEtsyItemsService CreateItemsService()
		{
			return new EtsyItemsService( _config );
		}

		/// <summary>
		///	Returns service to work with credentials
		/// </summary>
		/// <returns></returns>
		public IEtsyAuthenticationService CreateAuthenticationService()
		{
			return new EtsyAuthenticationService( _config );
		}

		/// <summary>
		///	Returns service to work with Etsy's receipts
		/// </summary>
		/// <returns></returns>
		public IEtsyOrdersService CreateOrdersService( )
		{
			return new EtsyOrdersService( _config );
		}
	}
}
