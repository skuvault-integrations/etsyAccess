using System;
using System.Collections.Generic;
using System.Text;

namespace EtsyAccess.Services
{
	public class BaseService
	{
		private readonly string _applicationKey;
		private readonly string _sharedSecret;

		public BaseService( string applicationKey, string sharedSecret )
		{
			_applicationKey = applicationKey;
			_sharedSecret = sharedSecret;
		}
	}
}
