using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EtsyAccess.Models;

namespace EtsyAccess.Services.Orders
{
	public interface IOrdersService
	{
		/// <summary>
		///	Returns orders that have changes at specified period
		/// </summary>
		/// <param name="startDate"></param>
		/// <param name="endDate"></param>
		/// <returns></returns>
		IEnumerable< Receipt > GetOrders( DateTime startDate, DateTime endDate );
		
		/// <summary>
		///	Returns orders asynchronously that were updated in specified period
		/// </summary>
		/// <param name="startDate"></param>
		/// <param name="endDate"></param>
		/// <returns></returns>
		Task< IEnumerable< Receipt > > GetOrdersAsync( DateTime startDate, DateTime endDate );
	}
}
