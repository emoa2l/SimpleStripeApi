namespace SimpleStripeApi.Models;

/// <summary>
/// Response model for a Stripe transaction
/// </summary>
public class StripeTransactionResponse
{
    /// <summary>
    /// Indicates if the transaction was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Stripe Payment Intent ID
    /// </summary>
    public string? PaymentIntentId { get; set; }

    /// <summary>
    /// Transaction status (succeeded, requires_action, requires_payment_method, etc.)
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Amount charged in cents
    /// </summary>
    public long? Amount { get; set; }

    /// <summary>
    /// Currency used
    /// </summary>
    public string? Currency { get; set; }

    /// <summary>
    /// Error message if transaction failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Client secret for handling additional actions (3D Secure, etc.)
    /// </summary>
    public string? ClientSecret { get; set; }
}
