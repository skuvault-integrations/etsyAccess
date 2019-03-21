using System;
using System.Collections.Generic;
using System.Text;
using CuttingEdge.Conditions;

namespace EtsyAccess.Models.Configuration
{
	public sealed class EtsyConfig
	{
		/// <summary>
		///	Tenant shop id
		/// </summary>
		public int? ShopId { get; private set; }
		/// <summary>
		///	Consumer key in OAuth 1.0 terms
		/// </summary>
		public string ApplicationKey { get; private set; }
		/// <summary>
		///	Consumer secret in OAuth 1.0 terms
		/// </summary>
		public string SharedSecret { get; private set; }
		/// <summary>
		///	Permanent token
		/// </summary>
		public string Token { get; private set; }
		/// <summary>
		///	Permanent token secret
		/// </summary>
		public string TokenSecret { get; private set; }

		public EtsyConfig( string applicationKey, string sharedSecret )
		{
			Condition.Requires( applicationKey ).IsNotNullOrEmpty();
			Condition.Requires( sharedSecret ).IsNotNullOrEmpty();

			ApplicationKey = applicationKey;
			SharedSecret = sharedSecret;
		}

		public EtsyConfig( string applicationKey, string sharedSecret, int? shopId, string token, string tokenSecret )
		{
			Condition.Requires( applicationKey ).IsNotNullOrEmpty();
			Condition.Requires( sharedSecret ).IsNotNullOrEmpty();
			Condition.Requires( shopId ).IsGreaterThan( 0 );
			Condition.Requires( token ).IsNotNullOrEmpty();
			Condition.Requires( tokenSecret ).IsNotNullOrEmpty();

			ApplicationKey = applicationKey;
			SharedSecret = sharedSecret;
			ShopId = shopId;
			Token = token;
			TokenSecret = tokenSecret;
		}
	}
}
