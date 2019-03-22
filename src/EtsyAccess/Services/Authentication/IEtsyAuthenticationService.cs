using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EtsyAccess.Services.Authentication
{
	public interface IEtsyAuthenticationService
	{
		Task< OAuthCredentials > GetTemporaryCredentials( string[] scopes );
		Task< OAuthCredentials > GetPermanentCredentials( string temporaryToken, string temporaryTokenSecret, string verifierCode );
	}
}
