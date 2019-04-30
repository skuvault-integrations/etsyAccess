using EtsyAccess.Models;
using NUnit.Framework;

namespace EtsyAccessTests
{
	public class ModelsTests
	{
		[Test]
		public void GivenListingOptionalFieldsLeftBlank_WhenInitializeListing_ThenItInitializes()
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
 
	}
}
