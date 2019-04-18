using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EtsyAccess.Models;

namespace EtsyAccess.Services.Items
{
	public interface IEtsyItemsService
	{
		/// <summary>
		/// Updates sku quantity
		/// </summary>
		/// <param name="sku">Sku</param>
		/// <param name="quantity">Quantity</param>
		/// <param name="token">Quantity</param>
		void UpdateSkuQuantity( string sku, int quantity, CancellationToken token );
		
		/// <summary>
		/// Updates sku quantity asynchronously
		/// </summary>
		/// <param name="sku"></param>
		/// <param name="quantity"></param>
		/// <param name="token"></param>
		/// <returns></returns>
		Task UpdateSkuQuantityAsync( string sku, int quantity, CancellationToken token );

		/// <summary>
		///	Updates skus quantities asynchronously
		/// </summary>
		/// <param name="skusQuantities">new quantity for each sku</param>
		/// <param name="token"></param>
		/// <returns></returns>
		Task UpdateSkusQuantityAsync( Dictionary< string, int > skusQuantities, CancellationToken token );

		/// <summary>
		/// Returns product inventory
		/// </summary>
		/// <param name="sku"></param>
		/// <param name="token"></param>
		/// <returns></returns>
		Task< ListingProduct > GetListingProductBySku( string sku, CancellationToken token );
	}
}
