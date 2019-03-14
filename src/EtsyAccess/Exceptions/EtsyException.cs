using System;
using System.Collections.Generic;
using System.Text;

namespace EtsyAccess.Exceptions
{
	public class EtsyException : Exception
	{
		public EtsyException( string message, Exception exception ) : base ( message, exception ) { }
		public EtsyException( string message ) : this ( message, null ) { }
	}

	public class EtsyInvalidSignatureException : EtsyException
	{
		public EtsyInvalidSignatureException( string message, Exception exception ) : base( message, exception ) { }
		public EtsyInvalidSignatureException( string message ) : base( message ) { }
	}
}
