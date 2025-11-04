using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SimpleStripeApi.Models;
using SimpleStripeApi.Services;

namespace SimpleStripeApi.Extensions;

/// <summary>
/// Extension methods for mapping Stripe API routes
/// </summary>
public static class EndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps Stripe transaction endpoints to the application
    /// </summary>
    /// <param name="endpoints">Endpoint route builder</param>
    /// <param name="routePrefix">Optional route prefix (default: "api/stripe")</param>
    /// <returns>Route group builder for additional configuration</returns>
    public static RouteGroupBuilder MapStripeTransactionEndpoints(
        this IEndpointRouteBuilder endpoints,
        string routePrefix = "api/stripe")
    {
        var group = endpoints.MapGroup(routePrefix)
            .WithTags("Stripe Transactions");

        group.MapPost("/transaction", ProcessTransactionAsync)
            .WithName("ProcessStripeTransaction")
            .WithOpenApi()
            .Produces<StripeTransactionResponse>(StatusCodes.Status200OK)
            .Produces<StripeTransactionResponse>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        return group;
    }

    private static async Task<IResult> ProcessTransactionAsync(
        StripeTransactionRequest request,
        IStripeTransactionService transactionService,
        CancellationToken cancellationToken)
    {
        // Validate request
        if (request.Amount <= 0)
        {
            return Results.BadRequest(new StripeTransactionResponse
            {
                Success = false,
                ErrorMessage = "Amount must be greater than zero"
            });
        }

        if (string.IsNullOrWhiteSpace(request.PaymentMethodId))
        {
            return Results.BadRequest(new StripeTransactionResponse
            {
                Success = false,
                ErrorMessage = "PaymentMethodId is required"
            });
        }

        if (string.IsNullOrWhiteSpace(request.Currency))
        {
            return Results.BadRequest(new StripeTransactionResponse
            {
                Success = false,
                ErrorMessage = "Currency is required"
            });
        }

        // Process transaction
        var response = await transactionService.ProcessTransactionAsync(request, cancellationToken);

        // Return appropriate status code based on success
        return response.Success
            ? Results.Ok(response)
            : Results.BadRequest(response);
    }
}
