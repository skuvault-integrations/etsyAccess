using Newtonsoft.Json;

namespace EtsyAccess.Models
{
	/// <summary>
	/// Represents the sale of a single item
	/// </summary>
	public class Transaction
	{
		/// <summary>
		/// The numeric ID for this transaction
		/// </summary>
		[JsonProperty("transaction_id")]
		public long Id { get; set; }
		/// <summary>
		/// The title of the listing for this transaction
		/// </summary>
		[JsonProperty("title")]
		public string ListingTitle { get; set; }
		/// <summary>
		/// The description of the listing for this transaction
		/// </summary>
		[JsonProperty("description")]
		public string ListingDescription { get; set; }
		/// <summary>
		/// The numeric ID for the seller of this transaction
		/// </summary>
		[JsonProperty("seller_user_id")]
		public int SellerUserId { get; set; }
		/// <summary>
		/// The numeric ID for the buyer of this transaction
		/// </summary>
		[JsonProperty("buyer_user_id")]
		public int BuyerUserId { get; set; }
		/// <summary>
		/// The date and time the transaction was created, in epoch seconds
		/// </summary>
		[JsonProperty("creation_tsz")]
		public long CreationTsz { get; set; }
		/// <summary>
		/// The date and time the transaction was paid, in epoch seconds
		/// </summary>
		[JsonProperty("paid_tsz")]
		public long? PaidTsz { get; set; }
		/// <summary>
		/// The date and time the transaction was shipped, in epoch seconds
		/// </summary>
		[JsonProperty("shipped_tsz")]
		public long? ShippedTsz { get; set; }
		/// <summary>
		/// The price of the transaction
		/// </summary>
		[JsonProperty("price")]
		public float Price { get; set; }
		/// <summary>
		/// The ISO code (alphabetic) for the seller's native currency
		/// </summary>
		[JsonProperty("currency_code")]
		public string CurrencyCode { get; set; }
		/// <summary>
		/// The quantity of items in this transaction
		/// </summary>
		[JsonProperty("quantity")]
		public int Quantity { get; set; }
		/// <summary>
		/// The tags in the listing for this transaction
		/// </summary>
		[JsonProperty("tags")]
		public string[] Tags { get; set; }
		/// <summary>
		/// The materials in the listing for this transaction
		/// </summary>
		[JsonProperty("materials")]
		public string[] Materials { get; set; }
		/// <summary>
		/// The numeric ID of the primary listing image for this transaction
		/// </summary>
		[JsonProperty("image_listing_id")]
		public long ImageListingId { get; set; }
		/// <summary>
		/// The numeric ID for the receipt associated to this transaction
		/// </summary>
		[JsonProperty("receipt_id")]
		public long ReceiptId { get; set; }
		/// <summary>
		/// The shipping cost for this transaction
		/// </summary>
		[JsonProperty("shipping_cost")]
		public float ShippingCost { get; set; }
		/// <summary>
		/// True if this listing is for a digital download
		/// </summary>
		[JsonProperty("is_digital")]
		public bool IsDigital { get; set; }
		/// <summary>
		/// Written description of the files attached to this digital listing
		/// </summary>
		[JsonProperty("file_data")]
		public string FileData { get; set; }
		/// <summary>
		/// The numeric ID for this listing associated to this transaction
		/// </summary>
		[JsonProperty("listing_id")]
		public long ListingId { get; set; }
		/// <summary>
		/// True if this transaction was created for an in-person quick sale.
		/// </summary>
		[JsonProperty("is_quick_sale")]
		public bool IsQuickSale { get; set; }
		/// <summary>
		/// The numeric ID of seller's feedback
		/// </summary>
		[JsonProperty("seller_feedback_id")]
		public long? SellerFeedbackId { get; set; }
		/// <summary>
		/// The numeric ID for the buyer's feedback
		/// </summary>
		[JsonProperty("buyer_feedback_id")]
		public long? BuyerFeedbackId { get; set; }
		/// <summary>
		/// The type of transaction, usually "listing"
		/// </summary>
		[JsonProperty("transaction_type")]
		public string TransactionType { get; set; }
		/// <summary>
		/// URL of this transaction
		/// </summary>
		[JsonProperty("url")]
		public string Url { get; set; }
		[JsonProperty("variations")]
		public ListingInventory[] Variations { get; set; }
		/// <summary>
		/// The product data with the purchased item, if any
		/// </summary>
		[JsonProperty("product_data")]
		public ListingProduct ProductData { get; set; }
	}
}
