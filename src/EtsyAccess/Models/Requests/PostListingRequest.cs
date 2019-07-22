using CuttingEdge.Conditions;

namespace EtsyAccess.Models.Requests
{
	public class PostListingRequest
	{
		public int Quantity { get; private set; }
		public string Title { get; private set; }
		public string Description { get; private set; }
		public float Price { get; private set; }
		public WhoMadeEnum WhoMade { get; private set; }
		public bool IsSupply { get; private set; }
		public string WhenMade { get; private set; }
		public long ShippingTemplateId { get; private set; }

		public StateEnum State { get; set; }

		public PostListingRequest( int quantity, string title, string description, float price, WhoMadeEnum whoMade, bool isSupply, string whenMade, long shippingTemplateId )
		{
			Condition.Requires( quantity, "quantity" ).IsGreaterThan( 0 );
			Condition.Requires( title, "title" ).IsNotNullOrWhiteSpace();
			Condition.Requires( description, "description" ).IsNotNullOrWhiteSpace();
			Condition.Requires( price, "price" ).IsGreaterThan( 0 );
			Condition.Requires( whenMade, "whenMade" ).IsNotNullOrWhiteSpace();
			Condition.Requires( shippingTemplateId, "shippingTemplateId" ).IsGreaterThan( 0 );

			this.Quantity = quantity;
			this.Title = title;
			this.Description = description;
			this.Price = price;
			this.WhoMade = whoMade;
			this.IsSupply = isSupply;
			this.WhenMade = whenMade;
			this.ShippingTemplateId = shippingTemplateId;
		}
	}

	public enum WhoMadeEnum { i_did, collective, someone_else }
	public enum StateEnum { active, draft }
}
