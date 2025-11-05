using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimpleStripeApi.Extensions;
using SimpleStripeApi.Models;
using Xunit;
using Xunit.Abstractions;

namespace SimpleStripeApi.Tests;

/// <summary>
/// Tests for route configuration options and customization
/// </summary>
public class RouteConfigurationTests
{
    private readonly ITestOutputHelper _output;

    public RouteConfigurationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    private static WebApplicationFactory<Program> CreateFactoryWithRoutePrefix(string routePrefix)
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string>
                    {
                        ["Stripe:SecretKey"] = "sk_test_placeholder_for_validation_test"
                    }!);
                });

                // We can't easily reconfigure routing in TestApi, so we'll use a custom host
                builder.UseTestServer();
                builder.ConfigureServices(services =>
                {
                    services.AddStripeTransactionService();
                    services.AddRouting();
                });

                builder.Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapStripeTransactionEndpoints(routePrefix);
                    });
                });
            });
    }

    [Fact]
    public async Task DefaultRoute_WorksAtApiStripeTransaction()
    {
        // Arrange
        using var factory = CreateFactoryWithRoutePrefix("api/stripe");
        var client = factory.CreateClient();

        var request = new StripeTransactionRequest
        {
            Amount = 0, // Will fail validation, but proves endpoint exists
            Currency = "usd",
            PaymentMethodId = "pm_card_visa"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/stripe/transaction", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var result = await response.Content.ReadFromJsonAsync<StripeTransactionResponse>();
        result.Should().NotBeNull();
        result!.ErrorMessage.Should().Contain("Amount");

        _output.WriteLine($"✓ Default route /api/stripe/transaction is accessible");
    }

    [Fact]
    public async Task CustomRoutePrefix_WorksAtCustomPath()
    {
        // Arrange
        using var factory = CreateFactoryWithRoutePrefix("payments/stripe");
        var client = factory.CreateClient();

        var request = new StripeTransactionRequest
        {
            Amount = 0, // Will fail validation
            Currency = "usd",
            PaymentMethodId = "pm_card_visa"
        };

        // Act
        var response = await client.PostAsJsonAsync("/payments/stripe/transaction", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var result = await response.Content.ReadFromJsonAsync<StripeTransactionResponse>();
        result.Should().NotBeNull();
        result!.ErrorMessage.Should().Contain("Amount");

        _output.WriteLine($"✓ Custom route /payments/stripe/transaction is accessible");
    }

    [Fact]
    public async Task RootLevelRoute_WorksAtRootTransaction()
    {
        // Arrange
        using var factory = CreateFactoryWithRoutePrefix("");
        var client = factory.CreateClient();

        var request = new StripeTransactionRequest
        {
            Amount = 0, // Will fail validation
            Currency = "usd",
            PaymentMethodId = "pm_card_visa"
        };

        // Act
        var response = await client.PostAsJsonAsync("/transaction", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var result = await response.Content.ReadFromJsonAsync<StripeTransactionResponse>();
        result.Should().NotBeNull();
        result!.ErrorMessage.Should().Contain("Amount");

        _output.WriteLine($"✓ Root level route /transaction is accessible");
    }

    [Fact]
    public async Task DefaultRoute_NotFoundAtWrongPath()
    {
        // Arrange
        using var factory = CreateFactoryWithRoutePrefix("api/stripe");
        var client = factory.CreateClient();

        var request = new StripeTransactionRequest
        {
            Amount = 1000,
            Currency = "usd",
            PaymentMethodId = "pm_card_visa"
        };

        // Act
        var response = await client.PostAsJsonAsync("/wrong/path/transaction", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        _output.WriteLine($"✓ Wrong path returns 404 as expected");
    }

    [Fact]
    public void MapStripeTransactionEndpoints_ReturnsRouteGroupBuilder()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddStripeTransactionService();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string>
        {
            ["Stripe:SecretKey"] = "sk_test_placeholder"
        }!);

        var app = builder.Build();

        // Act
        var result = app.MapStripeTransactionEndpoints();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeAssignableTo<RouteGroupBuilder>();

        _output.WriteLine($"✓ MapStripeTransactionEndpoints returns RouteGroupBuilder for method chaining");
    }

}
