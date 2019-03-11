using System;
using System.Collections.Generic;
using System.Text;
using Netco.Logging;

namespace EtsyAccess.Misc
{
	public class EtsyLogger
	{
		public static ILogger Log()
		{
			return NetcoLogger.GetLogger( "EtsyLogger" );
		}
	}
}
