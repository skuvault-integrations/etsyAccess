using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using CuttingEdge.Conditions;
using EtsyAccess.Exceptions;
using EtsyAccess.Shared;
using Polly;

namespace EtsyAccess.Models.Throttling
{
	public class Throttler
	{
		private readonly int _retryAttempts;

		public Throttler( int attempts )
		{
			Condition.Requires( attempts ).IsGreaterThan( 0 );

			_retryAttempts = attempts;
		}

		public async Task< TResult > Execute< TResult >( Func< Task< TResult > > funcToThrottle, Action< TimeSpan, int > onRetryAttempt, Func< string > extraLogInfo, Action< Exception > onException )
		{
			return await Policy.Handle< EtsyNetworkException >()
				.WaitAndRetryAsync( _retryAttempts,
					retryAttempt => TimeSpan.FromSeconds( Math.Pow( 2, retryAttempt ) ),
					( entityRaw, timeSpan, retryCount, context ) =>
					{
						onRetryAttempt?.Invoke( timeSpan, retryCount );
					})
				.ExecuteAsync(async () =>
				{
					try
					{
						return await funcToThrottle();
					}
					catch ( Exception exception )
					{
						EtsyException etsyException = null;

						string exceptionDetails = string.Empty;

						if ( extraLogInfo != null )
							exceptionDetails = extraLogInfo();

						if ( exception is EtsyServerException )
							etsyException = new EtsyException( exceptionDetails, exception );
						else
							etsyException = new EtsyNetworkException( exceptionDetails, exception );

						throw etsyException;
					}
					
				});
		}
	}
}
