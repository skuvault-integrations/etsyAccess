using EtsyAccess.Services.Authentication;
using FluentAssertions;
using NUnit.Framework;

namespace EtsyAccessTests
{
	[ TestFixture ]
	public class OAuthTests
	{
		private OAuthenticator _authenticator;

		[ SetUp ]
		public void Init()
		{
			this._authenticator = new OAuthenticator( "consumer key", "consumer secret" );
		}

		[ Test ]
		public void GivenJsonBodyWithOnlyASCIIChars_WhenPercentEncodeDataIsCalled_ThenCorrectStrIsExpected()
		{
			var jsonBody = "{\"product_id\":3882915049,\"sku\":\"testsku1-1\"}";

			var percentEncodedStr = this._authenticator.PercentEncodeData( jsonBody );
			percentEncodedStr.Should().Be( "%7B%22product_id%22%3A3882915049%2C%22sku%22%3A%22testsku1-1%22%7D" );
		}

		[ Test ]
		public void GivenJsonBodyWithNotASCIIChars_WhenPercentEncodeDataIsCalled_ThenCorrectStrIsExpected()
		{
			var jsonBody = "{\"property_id\":1,\"property_name\":\"Height\",\"scale_id\":1,\"scale_name\":\"inches\",\"value_ids\":[1021012107079],\"values\":[\"1.5”–1.7”\"]}";

			var percentEncodedStr = this._authenticator.PercentEncodeData( jsonBody );
			percentEncodedStr.Should().Be( "%7B%22property_id%22%3A1%2C%22property_name%22%3A%22Height%22%2C%22scale_id%22%3A1%2C%22scale_name%22%3A%22inches%22%2C%22value_ids%22%3A%5B1021012107079%5D%2C%22values%22%3A%5B%221.5%E2%80%9D%E2%80%931.7%E2%80%9D%22%5D%7D" );
		}
	}
}