using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CuttingEdge.Conditions;
using EtsyAccess.Exceptions;
using EtsyAccess.Models;
using EtsyAccess.Models.Configuration;
using EtsyAccess.Models.Throttling;
using EtsyAccess.Shared;

namespace EtsyAccess.Services.Common
{
	public class EtsyAdminService : BaseService, IEtsyAdminService
	{
		public EtsyAdminService( EtsyConfig config, Throttler throttler ) : base( config, throttler )
		{ }

		/// <summary>
		///	Returns shop info
		/// </summary>
		/// <param name="shopName">Etsy's shop name</param>
		/// <param name="token">Token for cancelling calls to endpoint</param>
		/// <returns></returns>
		public async Task< Shop > GetShopInfo( string shopName, CancellationToken token )
		{
			Condition.Requires( shopName ).IsNotNullOrEmpty();

			var mark = Mark.CreateNew();
			IEnumerable< Shop > response = null;
			string url = String.Format( EtsyEndPoint.GetShopInfoUrl, shopName );

			try
			{
				EtsyLogger.LogStarted( this.CreateMethodCallInfo( url, mark, additionalInfo : this.AdditionalLogInfo() ) );

				response = await GetEntitiesAsync< Shop >( url, token ).ConfigureAwait( false );

				EtsyLogger.LogEnd( this.CreateMethodCallInfo( url, mark, methodResult: response.ToJson(), additionalInfo : this.AdditionalLogInfo() ) );
			}
			catch ( Exception exception )
			{
				var etsyException = new EtsyException( this.CreateMethodCallInfo( url, mark, additionalInfo : this.AdditionalLogInfo() ), exception );
				EtsyLogger.LogTraceException( etsyException );
				throw etsyException;
			}

			return response.FirstOrDefault();
		}
	}
}
