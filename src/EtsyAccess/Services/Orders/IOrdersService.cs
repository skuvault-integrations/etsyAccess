using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EtsyAccess.Services.Orders
{
	public interface IOrdersService
	{
		/// <summary>
		///	Returns orders that have changes at specified period
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="startDate"></param>
		/// <param name="endDate"></param>
		/// <returns></returns>
		IEnumerable< T > GetOrders< T >( DateTime startDate, DateTime endDate );
		/// <summary>
		///	Returns orders asynchronously that have changes at specified period
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="startDate"></param>
		/// <param name="endDate"></param>
		/// <returns></returns>
		Task< IEnumerable< T > > GetOrdersAsync< T >(DateTime startDate, DateTime endDate);
	}
}
