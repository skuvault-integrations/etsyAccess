using Newtonsoft.Json;

namespace EtsyAccess.Models
{
	/// <summary>
	/// Listings on Etsy are items for sale. Each Listing is associated with a User, and with a Shop (Users own Shops.)
	/// Etsy Listings have a creation date, and are valid for approximately four months.
	/// Listings have a price and a quantity, and when they're sold out, the User must renew them before they can be sold again.
	/// </summary>
	public class Listing
	{
		/// <summary>
		/// The listing's numeric ID
		/// </summary>
		[JsonProperty("listing_id")]
		public long Id { get; set; }
		/// <summary>
		/// One of active, removed, sold_out, expired, alchemy, edit, create, private, or unavailable.
		/// </summary>
		[JsonProperty("state")]
		public string State { get; set; }
		/// <summary>
		/// The numeric ID of the user who posted the item
		/// </summary>
		[JsonProperty("user_id")]
		public int UserId { get; set; }
		/// <summary>
		/// The numeric ID of the listing's category.
		/// </summary>
		[JsonProperty("category_id")]
		public int? CategoryId { get; set; }
		/// <summary>
		/// The listing's title
		/// </summary>
		[JsonProperty("title")]
		public string Title { get; set; }
		/// <summary>
		/// A description of the item
		/// </summary>
		[JsonProperty("description")]
		public string Description { get; set; }
		/// <summary>
		/// Creation time, in epoch seconds
		/// </summary>
		[JsonProperty("creation_tsz")]
		public long CreationTsz { get; set; }
		/// <summary>
		/// The listing's expiration date and time, in epoch seconds
		/// </summary>
		[JsonProperty("ending_tsz")]
		public long EndingTsz { get; set; }
		/// <summary>
		/// The date and time the listing was originally posted, in epoch seconds
		/// </summary>
		[JsonProperty("original_creation_tsz")]
		public long OriginalCreationTsz { get; set; }
		/// <summary>
		/// The date and time the listing was updated, in epoch seconds
		/// </summary>
		[JsonProperty("last_modified_tsz")]
		public long LastModifiedTsz { get; set; }
		/// <summary>
		/// The item's price (will be treated as private for sold listings)
		/// </summary>
		[JsonProperty("price")]
		public string Price { get; set; }
		/// <summary>
		/// The ISO (alphabetic) code for the item's currency
		/// </summary>
		[JsonProperty("currency_code")]
		public string CurrencyCode { get; set; }
		/// <summary>
		/// The quantity of this item available for sale
		/// </summary>
		[JsonProperty("quantity")]
		public int Quantity { get; set; }
		/// <summary>
		/// A list of distinct SKUs applied to a listing
		/// </summary>
		[JsonProperty("sku")]
		public string[] Sku { get; set; }
		/// <summary>
		/// A list of tags for the item
		/// </summary>
		[JsonProperty("tags")]
		public string[] Tags { get; set; }
		/// <summary>
		/// Name of the category of the item and the names of categories in that hierarchy
		/// </summary>
		[JsonProperty("category_path")]
		public string[] CategoryPath { get; set; }
		/// <summary>
		/// The numeric ID of each category in the this Listing's category hierarchy
		/// </summary>
		[JsonProperty("category_path_ids")]
		public int[] CategoryPathIds { get; set; }
		/// <summary>
		/// The seller taxonomy id of the listing
		/// </summary>
		[JsonProperty("taxonomy_id")]
		public int? TaxonomyId { get; set; }
		/// <summary>
		/// Etsy's suggestion for the seller taxonomy_id for this listing.
		/// Etsy makes this suggestion if the listing does not have a taxonomy_id chosen by the seller.
		/// </summary>
		[JsonProperty("suggested_taxonomy_id")]
		public int SuggestedTaxonomyId { get; set; }
		/// <summary>
		/// The name of the taxonomy node of the item and the names of that node's parents
		/// </summary>
		[JsonProperty("taxonomy_path")]
		public string[] TaxonomyPath { get; set; }
		/// <summary>
		/// A list of materials used in the item
		/// </summary>
		[JsonProperty("materials")]
		public string[] Materials { get; set; }
		/// <summary>
		/// The numeric ID of the shop section for this listing, can be null
		/// </summary>
		[JsonProperty("shop_section_id")]
		public int? ShopSectionId { get; set; }
		/// <summary>
		/// The time at which the listing last changed state.
		/// </summary>
		[JsonProperty("state_tsz")]
		public long StateTsz { get; set; }
		/// <summary>
		/// The full URL to the listing's page on Etsy
		/// </summary>
		[JsonProperty("url")]
		public string Url { get; set; }
		/// <summary>
		/// The number of times the listing has been viewed on Etsy.com (does not include API views).
		/// </summary>
		[JsonProperty("views")]
		public int Views { get; set; }
		/// <summary>
		/// The number of members who've marked this Listing as a favorite
		/// </summary>
		[JsonProperty("num_favorers")]
		public int FavourersNumber { get; set; }
		/// <summary>
		/// The minimum number of days for processing for this listing
		/// </summary>
		[JsonProperty("processing_min")]
		public int? ProcessingMin { get; set; }
		/// <summary>
		/// The maximum number of days for processing for this listing
		/// </summary>
		[JsonProperty("processing_max")]
		public int? ProcessingMax { get; set; }
		/// <summary>
		/// Who made the item being listed
		/// </summary>
		[JsonProperty("who_made")]
		public string WhoMade { get; set; }
		/// <summary>
		/// True if the listing is a supply
		/// </summary>
		[JsonProperty("is_supply")]
		public bool IsSupply { get; set; }
		/// <summary>
		/// When was the item made
		/// </summary>
		[JsonProperty("when_made")]
		public string WhenMade { get; set; }
		/// <summary>
		/// How much the item weighs
		/// </summary>
		[JsonProperty("item_weight")]
		public float? ItemWeight { get; set; }
		/// <summary>
		/// The units used to represent the weight of this item
		/// </summary>
		[JsonProperty("item_weight_unit")]
		public ItemWeightUnit? ItemWeightUnit { get; set; }
		/// <summary>
		/// How long the item is
		/// </summary>
		[JsonProperty("item_length")]
		public float? ItemLength { get; set; }
		/// <summary>
		/// How wide the item is
		/// </summary>
		[JsonProperty("item_width")]
		public float? ItemWidth { get; set; }
		/// <summary>
		/// How tall the item is
		/// </summary>
		[JsonProperty("item_height")]
		public float? ItemHeight { get; set; }
		/// <summary>
		/// The units used to represent the dimensions of this item
		/// </summary>
		[JsonProperty("item_dimensions_unit")]
		public ItemDimensionsUnit? ItemDimensionsUnit { get; set; }
		/// <summary>
		/// Is this listing a private listing that is intended for a specific buyer and hidden from shop view.
		/// </summary>
		[JsonProperty("is_private")]
		public bool IsPrivate { get; set; }
		/// <summary>
		/// Who is this listing for
		/// </summary>
		[JsonProperty("recipient")]
		public string Recipient { get; set; }
		/// <summary>
		/// What is the occasion for this listing
		/// </summary>
		[JsonProperty("occasion")]
		public string Occasion { get; set; }
		/// <summary>
		/// Style of this listing. Each style is a free-form text string such as "Formal", or "Steampunk".
		/// </summary>
		[JsonProperty("style")]
		public string[] Style { get; set; }
		/// <summary>
		/// If this flag is true, any applicable shop tax rates will not be applied to this listing on checkout.
		/// </summary>
		[JsonProperty("non_taxable")]
		public bool NonTaxable { get; set; }
		/// <summary>
		/// If this flag is true, a buyer may contact the seller for a customized order. Can only be set when the shop accepts custom orders and defaults to true in that case.
		/// </summary>
		[JsonProperty("is_customizable")]
		public bool IsCustomizable { get; set; }
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
		/// True if this listing can be updated with the new inventory endpoints
		/// </summary>
		[JsonProperty("can_write_inventory")]
		public bool CanWriteInventory { get; set; }
		/// <summary>
		/// True if variations are available for this Listing
		/// </summary>
		[JsonProperty("has_variations")]
		public bool HasVariations { get; set; }
		/// <summary>
		/// True if this listing has been set to automatic renewals
		/// </summary>
		[JsonProperty("should_auto_renew")]
		public bool ShouldAutoRenew { get; set; }
		/// <summary>
		/// The IETF language tag (e.g. 'fr') for the language in which the listing is returned
		/// </summary>
		[JsonProperty("language")]
		public string Language { get; set; }
	}

	public enum ItemWeightUnit
	{
		Oz, Lb, G, Kg
	}

	public enum ItemDimensionsUnit
	{
		In, Ft, Mm, Cm, M
	}
}
