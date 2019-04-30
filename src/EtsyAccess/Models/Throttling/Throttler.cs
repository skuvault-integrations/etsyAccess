using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EtsyAccess.Exceptions;
using Netco.ThrottlerServices;

namespace EtsyAccess.Models.Throttling
{
	public sealed class Throttler : IDisposable
	{
		public int MaxQuota
		{
			get { return _maxQuota;  }
		}
		public int RemainingQuota
		{
			get { return _remainingQuota; }
		}

		/// <summary>
		///	API limits (total per day)
		/// </summary>
		public int DayLimit { get; set; }
		/// <summary>
		///	API requests remaining
		/// </summary>
		public int DayLimitRemaining { get; set; }

		private readonly int _maxQuota;
		private readonly int _quotaRestoreTimeInSeconds;
		private readonly int _maxRetryCount;
		private volatile int _remainingQuota;
		private readonly Timer _timer;
		private bool _timerStarted = false;
		private object _lock = new object();

		/// <summary>
		/// Throttler constructor. See code section for details
		/// </summary>
		/// <code>
		/// // Maximum request quota: 10 requests per 1 second 10 retry attempts if API limit exceeded
		/// var throttler = new Throttler( 10, 1, 10 )
		/// </code>
		/// <param name="maxQuota">Max requests per restore time interval</param>
		/// <param name="quotaRestoreTimeInSeconds">Quota restore time in seconds</param>
		/// <param name="maxRetryCount">Max Retry Count</param>
		public Throttler( int maxQuota, int quotaRestoreTimeInSeconds, int maxRetryCount = 10 )
		{
			this._maxQuota = this._remainingQuota = maxQuota;
			this._maxRetryCount = maxRetryCount;
			this._quotaRestoreTimeInSeconds = quotaRestoreTimeInSeconds;

			_timer = new Timer( RestoreQuota, null, Timeout.Infinite, _quotaRestoreTimeInSeconds * 1000 );
		}
		
		public async Task< TResult > ExecuteAsync< TResult >( Func< Task< TResult > > funcToThrottle )
		{
			lock ( _lock )
			{
				if ( !_timerStarted )
				{
					_timer.Change( _quotaRestoreTimeInSeconds * 1000, _quotaRestoreTimeInSeconds * 1000 );
					_timerStarted = true;
				}
			}

			var retryCount = 0;

			while( true )
			{
				try
				{
					return await this.TryExecuteAsync( funcToThrottle ).ConfigureAwait( false );
				}
				catch( Exception ex )
				{
					if (!( ex is EtsyApiLimitsExceeded || ex.InnerException is EtsyApiLimitsExceeded ))
						throw;

					if (retryCount >= this._maxRetryCount)
						throw;

					#if DEBUG
						Trace.WriteLine($"[{ DateTime.Now }] Got API requests exceeded error. Waiting... " );
					#endif

					this._remainingQuota = 0;
					await Task.Delay( _quotaRestoreTimeInSeconds * 1000 ).ConfigureAwait( false );
					retryCount++;
				}
			}
		}

		private async Task< TResult > TryExecuteAsync< TResult >( Func< Task< TResult > > funcToThrottle )
		{
			await this.WaitIfNeededAsync().ConfigureAwait( false );

			var result = await funcToThrottle().ConfigureAwait( false );

			return result;
		}

		private async Task WaitIfNeededAsync()
		{
			while ( true )
			{
				lock (_lock)
				{
					if (_remainingQuota > 0)
					{
						_remainingQuota--;
#if DEBUG
						Trace.WriteLine($"[{ DateTime.Now }] We have quota remains { _remainingQuota }. Continue work" );
#endif
						return;
					}
				}

#if DEBUG
				Trace.WriteLine($"[{ DateTime.Now }] Quota remain { _remainingQuota }. Waiting { _quotaRestoreTimeInSeconds } seconds to continue" );
#endif

				await Task.Delay( _quotaRestoreTimeInSeconds * 1000 ).ConfigureAwait( false );
			}
		}

		/// <summary>
		///	Releases quota that we have for each period of time
		/// </summary>
		/// <param name="state"></param>
		private void RestoreQuota( object state = null )
		{
			this._remainingQuota = this._maxQuota;

			#if DEBUG
				Trace.WriteLine($"[{ DateTime.Now }] Restored { _maxQuota } quota" );
			#endif
		}

		#region IDisposable Support
		private bool disposedValue = false;

		void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					_timer.Dispose();
				}

				disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}
		#endregion
	}
}
