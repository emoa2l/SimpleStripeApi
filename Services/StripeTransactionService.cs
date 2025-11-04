using Microsoft.Extensions.Configuration;
using SimpleStripeApi.Models;
using Stripe;

namespace SimpleStripeApi.Services;

/// <summary>
/// Service for processing Stripe transactions
/// </summary>
public class StripeTransactionService : IStripeTransactionService
{
    private readonly string _apiKey;

    public StripeTransactionService(IConfiguration configuration)
    {
        _apiKey = configuration["Stripe:SecretKey"]
            ?? throw new InvalidOperationException("Stripe:SecretKey configuration is missing");

        StripeConfiguration.ApiKey = _apiKey;
    }

    /// <summary>
    /// Process a single Stripe transaction asynchronously
    /// </summary>
    public async Task<StripeTransactionResponse> ProcessTransactionAsync(
        StripeTransactionRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = request.Amount,
                Currency = request.Currency,
                PaymentMethod = request.PaymentMethodId,
                Description = request.Description,
                ReceiptEmail = request.CustomerEmail,
                Confirm = true,
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true,
                    AllowRedirects = "never"
                }
            };

            if (request.Metadata != null && request.Metadata.Count > 0)
            {
                options.Metadata = request.Metadata;
            }

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options, cancellationToken: cancellationToken);

            return new StripeTransactionResponse
            {
                Success = paymentIntent.Status == "succeeded",
                PaymentIntentId = paymentIntent.Id,
                Status = paymentIntent.Status,
                Amount = paymentIntent.Amount,
                Currency = paymentIntent.Currency,
                ClientSecret = paymentIntent.ClientSecret
            };
        }
        catch (StripeException ex)
        {
            return new StripeTransactionResponse
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
        catch (Exception ex)
        {
            return new StripeTransactionResponse
            {
                Success = false,
                ErrorMessage = $"An unexpected error occurred: {ex.Message}"
            };
        }
    }
}
