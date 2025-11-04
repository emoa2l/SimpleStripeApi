using SimpleStripeApi.Models;

namespace SimpleStripeApi.Services;

/// <summary>
/// Interface for Stripe transaction service
/// </summary>
public interface IStripeTransactionService
{
    /// <summary>
    /// Process a single Stripe transaction asynchronously
    /// </summary>
    /// <param name="request">Transaction request details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Transaction response</returns>
    Task<StripeTransactionResponse> ProcessTransactionAsync(
        StripeTransactionRequest request,
        CancellationToken cancellationToken = default);
}
