using System;
using System.Collections.Generic;
using System.Text;

namespace EtsyAccess.Exceptions
{
	public class EtsyException : Exception
	{
		public EtsyException( string message, Exception exception ) : base( message, exception ) { }
		public EtsyException( string message ) : this ( message, null ) { }
	}

	/// <summary>
	///	Request to Etsy servers failed due to connection issues or specific server behavior.
	///	Normally operation can be reattempted after receiving this exception
	/// </summary>
	public class EtsyNetworkException : EtsyException
	{
		public EtsyNetworkException( string message, Exception exception ) : base( message, exception ) { }
		public EtsyNetworkException( string message ) : this( message, null ) { }
	}

	/// <summary>
	///	Etsy server exception
	/// </summary>
	public class EtsyServerException : EtsyException
	{
		/// <summary>
		///	Http response status code
		/// </summary>
		public int Code { get; private set; }

		public EtsyServerException( string message, int code, Exception exception ) : base( message, exception )
		{
			Code = code;
		}

		public EtsyServerException( string message, int code ) : this ( message, code, null ) { }
	}

	public class EtsyInvalidSignatureException : EtsyException
	{
		public EtsyInvalidSignatureException( string message ) : base( message, null ) { }
	}
}
