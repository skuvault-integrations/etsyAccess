using System;
using System.Collections.Generic;
using System.Text;

namespace EtsyAccess.Services
{
	public class BaseService
	{
		private readonly string _accessToken;
		private const string BaseApiUrl = "https://openapi.etsy.com/v2";

		public BaseService( string accessToken )
		{
			_accessToken = accessToken;
		}
	}
}
