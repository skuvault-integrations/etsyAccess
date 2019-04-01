using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EtsyAccess.Models;
using EtsyAccess.Models.Throttling;

namespace EtsyAccess.Services.Common
{
	public interface IEtsyAdminService
	{
		/// <summary>
		///	Returns shop info by name
		/// </summary>
		/// <param name="shopName"></param>
		/// <param name="token"></param>
		/// <returns></returns>
		Task< Shop > GetShopInfo( string shopName, CancellationToken token );
	}
}
