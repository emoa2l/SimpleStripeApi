using Microsoft.Extensions.DependencyInjection;
using SimpleStripeApi.Services;

namespace SimpleStripeApi.Extensions;

/// <summary>
/// Extension methods for registering Stripe services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Stripe transaction services to the DI container
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddStripeTransactionService(this IServiceCollection services)
    {
        services.AddScoped<IStripeTransactionService, StripeTransactionService>();
        return services;
    }
}
