using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EtsyAccess.Services.Orders
{
	public class OrdersService : BaseService, IOrdersService
	{
		public OrdersService( string applicationKey, string sharedSecret ) : base( applicationKey, sharedSecret )
		{ }

		public IEnumerable< T > GetOrders< T >( DateTime startDate, DateTime endDate )
		{
			throw new NotImplementedException();
		}

		public Task< IEnumerable< T > > GetOrdersAsync< T >( DateTime startDate, DateTime endDate )
		{
			throw new NotImplementedException();
		}
	}
}
