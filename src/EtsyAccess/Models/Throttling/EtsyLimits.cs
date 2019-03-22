using System;
using System.Collections.Generic;
using System.Text;

namespace EtsyAccess.Models.Throttling
{
	internal class EtsyLimits
	{
		public int TotalAvailableRequests { get; private set; }
		public int CallsRemaining{ get; private set; }

		public EtsyLimits( int totalAvailableRequests, int callRemaining )
		{
			TotalAvailableRequests = totalAvailableRequests;
			CallsRemaining = callRemaining;
		}
	}
}
