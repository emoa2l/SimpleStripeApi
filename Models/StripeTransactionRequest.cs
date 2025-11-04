namespace SimpleStripeApi.Models;

/// <summary>
/// Request model for processing a Stripe transaction
/// </summary>
public class StripeTransactionRequest
{
    /// <summary>
    /// Amount in cents (e.g., 1000 = $10.00)
    /// </summary>
    public long Amount { get; set; }

    /// <summary>
    /// Three-letter ISO currency code (e.g., "usd")
    /// </summary>
    public string Currency { get; set; } = "usd";

    /// <summary>
    /// Payment method ID or source token from Stripe.js
    /// </summary>
    public string PaymentMethodId { get; set; } = string.Empty;

    /// <summary>
    /// Optional description for the transaction
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Optional customer email
    /// </summary>
    public string? CustomerEmail { get; set; }

    /// <summary>
    /// Optional metadata as key-value pairs
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
}
