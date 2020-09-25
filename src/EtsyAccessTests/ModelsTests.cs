using EtsyAccess.Models;
using FluentAssertions;
using NUnit.Framework;
using System.Linq;

namespace EtsyAccessTests
{
	public class ModelsTests
	{
		[Test]
		public void Listing_WhenGivenListingOptionalFieldsLeftBlank_ThenItInitializesListing()
		{
			var listing = new Listing
			{
				ItemWeight = null,
				ItemLength = null,
				ItemWidth = null,
				ItemHeight = null,
				Tags = new string[]{},
				Materials = new string[]{},
				Sku = new string[]{}
			};

			Assert.IsNotNull( listing );
		}

		[ Test ]
		public void GivenPropertyValuesWithEncodedQuotes_WhenDecodeValuesQuotesAndEscapeCalled_ThenDecodedPropertyValuesAreReturned()
		{
			var propertyValue = new PropertyValue()
			{
				Id = 1,
				Values = new string[] { "10&quot;x4&quot;x&quot;2", "5&quot;x2&quot;x&quot;1" }
			};

			var decodedPropertyValues = propertyValue.DecodeValuesQuotesAndEscape();

			decodedPropertyValues.Values.First().Should().Be( "10\"x4\"x\"2" );
			decodedPropertyValues.Values.Last().Should().Be( "5\"x2\"x\"1" );
		}
	}
}
