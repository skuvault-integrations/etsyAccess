using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EtsyAccess.Services.Items
{
	public class ItemsService : BaseService, IItemsService
	{
		public ItemsService( string accessToken ) : base( accessToken )
		{ }

		public void UpdateSkuQuantity(string sku, int quantity)
		{
			throw new NotImplementedException();
		}

		Task IItemsService.UpdateSkuQuantityAsync(string sku, int quantity)
		{
			throw new NotImplementedException();
		}
	}
}
