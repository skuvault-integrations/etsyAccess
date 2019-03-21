using System;
using System.Collections.Generic;
using System.Text;
using EtsyAccess.Misc;
using Newtonsoft.Json;

namespace EtsyAccess.Models
{
	/// <summary>
	/// Represents proof of payment from a user to a shop for one or more transactions
	/// </summary>
	public class Receipt
	{
		/// <summary>
		/// The receipt's numeric ID
		/// </summary>
		[JsonProperty("receipt_id")]
		public int Id { get; set; }
		/// <summary>
		/// The enum of the order type this receipt is associated with.
		/// </summary>
		[JsonProperty("receipt_type")]
		public int Type { get; set; }
		/// <summary>
		/// The numeric ID of the order this receipt is associated with.
		/// </summary>
		[JsonProperty("order_id")]
		public int OrderId { get; set; }
		/// <summary>
		/// Creation time, in epoch seconds.
		/// </summary>
		[JsonProperty("creation_tsz")]
		public long CreationTsz { get; set; }
		/// <summary>
		/// Last modification time, in epoch seconds
		/// </summary>
		[JsonProperty("last_modified_tsz")]
		public long LastModifiedTsz { get; set; }
		/// <summary>
		///	The name portion of the buyer's address.
		/// </summary>
		[JsonProperty("name")]
		public string BuyerName { get; set; }
		/// <summary>
		/// The first line of the buyer's address.
		/// </summary>
		[JsonProperty("first_line")]
		public string BuyerAddressLine1 { get; set; }
		/// <summary>
		/// The second line of the buyer's address.
		/// </summary>
		[JsonProperty("second_line")]
		public string BuyerAddressLine2 { get; set; }
		/// <summary>
		/// The city for the buyer's address.
		/// </summary>
		[JsonProperty("city")]
		public string BuyerCity { get; set; }
		/// <summary>
		/// The state for the buyer's address.
		/// </summary>
		[JsonProperty("state")]
		public string BuyerState { get; set; }
		/// <summary>
		/// The zip code of the buyer's address
		/// </summary>
		[JsonProperty("zip")]
		public string BuyerZip { get; set; }
		/// <summary>
		/// The locally formatted address strng of the buyer's shipping address.
		/// </summary>
		[JsonProperty("formatted_address")]
		public string BuyerShippingFormattedAddress { get; set; }
		/// <summary>
		/// The numeric ID of the buyer's country.
		/// </summary>
		[JsonProperty("country_id")]
		public int? BuyerCountryId { get; set; }
		/// <summary>
		/// The payment method used. May be pp, cc, ck, mo, or other (stands for paypal, credit card, check, money order, other).
		/// </summary>
		[JsonProperty("payment_method")]
		public string PaymentMethod { get; set; }
		/// <summary>
		/// The email address where payment confirmation is sent.
		/// </summary>
		[JsonProperty("payment_email")]
		public string PaymentEmail { get; set; }
		/// <summary>
		/// An optional message from the seller
		/// </summary>
		[JsonIgnore]
		[JsonProperty("message_from_seller")]
		public string SellerMessage { get; set; }
		/// <summary>
		/// An optional message from the buyer
		/// </summary>
		[JsonIgnore]
		[JsonProperty("message_from_buyer")]
		public string BuyerMessage { get; set; }
		/// <summary>
		/// True if the receipt was paid.
		/// </summary>
		[JsonProperty("was_paid")]
		public bool WasPaid { get; set; }
		/// <summary>
		/// The total sales tax of the receipt
		/// </summary>
		[JsonProperty("total_tax_cost")]
		public float TotalTaxCost { get; set; }
		/// <summary>
		/// The total VAT of the receipt
		/// </summary>
		[JsonProperty("total_vat_cost")]
		public float TotalVatCost { get; set; }
		/// <summary>
		/// Sum of the individual listings' (price * quantity). Does not included tax or shipping costs
		/// </summary>
		[JsonProperty("total_price")]
		public float TotalPrice { get; set; }
		/// <summary>
		/// The total shipping cost of the receipt
		/// </summary>
		[JsonProperty("total_shipping_cost")]
		public float TotalShippingCost { get; set; }
		/// <summary>
		/// The ISO code (alphabetic) for the seller's native currency
		/// </summary>
		[JsonProperty("currency_code")]
		public string CurrencyCode { get; set; }
		/// <summary>
		/// 	The machine generated acknowledgement from the payment system
		/// </summary>
		[JsonProperty("message_from_payment")]
		public string PaymentSystemMessage { get; set; }
		/// <summary>
		/// True if the items in the receipt were shipped.
		/// </summary>
		[JsonProperty("was_shipped")]
		public bool WasShipped { get; set; }
		/// <summary>
		/// The email address of the buyer. Access to this field is granted on a case by case basis for third-party integrations that require full access. Sellers using private apps to manage their shops can still access this field
		/// </summary>
		[JsonProperty("buyer_email")]
		public string BuyerEmail { get; set; }
		/// <summary>
		/// The email address of the seller
		/// </summary>
		[JsonProperty("seller_email")]
		public string SellerEmail { get; set; }
		/// <summary>
		/// Whether the buyer marked item as a gift
		/// </summary>
		[JsonProperty("is_gift")]
		public bool IsGift { get; set; }
		/// <summary>
		/// Whether the buyer purchased gift wrap
		/// </summary>
		[JsonProperty("needs_gift_wrap")]
		public bool NeedsGiftWrap { get; set; }
		/// <summary>
		/// The message the buyer wants sent with the gift
		/// </summary>
		[JsonProperty("gift_message")]
		public string GiftMessage { get; set; }
		/// <summary>
		/// The gift wrap price of the receipt
		/// </summary>
		[JsonProperty("gift_wrap_price")]
		public float GiftWrapPrice { get; set; }
		/// <summary>
		/// The total discount for the receipt, if using a discount (percent or fixed) Coupon. Free shipping Coupons are not reflected here
		/// </summary>
		[JsonProperty("discount_amt")]
		public float DiscountAmt { get; set; }
		/// <summary>
		/// Total_price minus coupon discounts. Does not included tax or shipping cost
		/// </summary>
		[JsonProperty("subtotal")]
		public float SubTotal { get; set; }
		/// <summary>
		/// Total_price minus coupon discount plus tax and shipping costs
		/// </summary>
		[JsonProperty("grandtotal")]
		public float GrandTotal { get; set; }
		/// <summary>
		/// Grand total after payment adjustments
		/// </summary>
		[JsonProperty("adjusted_grandtotal")]
		public float AdjustedGrandTotal { get; set; }
		[JsonProperty("shipping_details")]
		public ShippingDetails ShippingDetails { get; set; }
		/// <summary>
		/// Shipment information associated to this receipt
		/// </summary>
		[JsonProperty("shipments")]
		public ReceiptShipment[] Shipments { get; set; }
		[JsonProperty("Transactions")]
		public Transaction[] Transactions { get; set; }
		[JsonProperty("Listings")]
		public Listing[] Listings { get; set; }
		[JsonProperty("Country")]
		public Country Country { get; set; }
		/// <summary>
		///	Receipt creation time UTC
		/// </summary>
		public DateTime CreateDateUtc => CreationTsz.FromEpochTime();
		/// <summary>
		///	Last modified date UTC
		/// </summary>
		public DateTime LastModifiedDateUtc => LastModifiedTsz.FromEpochTime();
	}

	/// <summary>
	///	Represents a distinct shipment for an order
	/// </summary>
	public class ReceiptShipment
	{
		/// <summary>
		/// Shipping carrier name
		/// </summary>
		public string CarrierName { get; set; }
		/// <summary>
		/// Receipt shipping id used internally
		/// </summary>
		public int ReceiptShippingId { get; set; }
		/// <summary>
		/// Tracking code for carrier
		/// </summary>
		public string TrackingCode { get; set; }
		/// <summary>
		/// Tracking URL for carrier's website
		/// </summary>
		public string TrackingUrl { get; set; }
		/// <summary>
		/// Optional note sent to buyer
		/// </summary>
		public string BuyerNote { get; set; }
		/// <summary>
		/// Date the notification was sent
		/// </summary>
		public long NotificationDate { get; set; }
	}

	public class ShippingDetails
	{
		[JsonProperty("can_mark_as_shipped")]
		public bool CanMarkAsShipped { get; set; }
		[JsonProperty("was_shipped")]
		public bool WasShipped { get; set; }
		[JsonProperty("is_future_shipment")]
		public bool IsFutureShipment { get; set; }
		[JsonProperty("has_free_shipping_discount")]
		public bool HasFreeShipmentDiscount { get; set; }
		[JsonProperty("not_shipped_state_display")]
		public string NotShippedStateDisplay { get; set; }
		[JsonProperty("shipping_method")]
		public string ShippingMethod { get; set; }
		[JsonProperty("is_estimated_delivery_date_relevant")]
		public bool IsEstimatedDeliveryDateRelevant { get; set; }
	}
}
