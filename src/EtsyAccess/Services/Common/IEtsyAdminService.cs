using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EtsyAccess.Models;
using EtsyAccess.Models.Throttling;

namespace EtsyAccess.Services.Common
{
	public interface IEtsyAdminService
	{
		Task<Shop> GetShopInfo(string shopName);
	}
}
