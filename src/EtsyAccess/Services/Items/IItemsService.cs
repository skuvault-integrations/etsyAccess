using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EtsyAccess.Services.Items
{
	public interface IItemsService
	{
		/// <summary>
		/// Updates sku quantity
		/// </summary>
		/// <param name="sku">Sku</param>
		/// <param name="quantity">Quantity</param>
		void UpdateSkuQuantity( string sku, int quantity );
		/// <summary>
		/// Updates sku quantity asynchronously
		/// </summary>
		/// <param name="sku"></param>
		/// <param name="quantity"></param>
		/// <returns></returns>
		Task UpdateSkuQuantityAsync( string sku, int quantity );
	}
}
