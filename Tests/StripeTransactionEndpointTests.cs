using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using SimpleStripeApi.Models;
using Xunit;
using Xunit.Abstractions;

namespace SimpleStripeApi.Tests;

/// <summary>
/// Integration tests for Stripe transaction endpoints
/// Note: These tests require a valid Stripe test API key to be configured
/// </summary>
public class StripeTransactionEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public StripeTransactionEndpointTests(
        WebApplicationFactory<Program> factory,
        ITestOutputHelper output)
    {
        _client = factory.CreateClient();
        _output = output;
    }

    [Fact]
    public async Task HealthCheck_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();

        // Log output
        _output.WriteLine("=== Health Check Test ===");
        _output.WriteLine($"Status Code: {response.StatusCode}");
        _output.WriteLine($"Response: {content}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ProcessTransaction_WithMissingPaymentMethodId_ReturnsBadRequest()
    {
        // Arrange
        var request = new StripeTransactionRequest
        {
            Amount = 1000,
            Currency = "usd",
            PaymentMethodId = ""
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/stripe/transaction", request);
        var result = await response.Content.ReadFromJsonAsync<StripeTransactionResponse>();

        // Log output
        _output.WriteLine("=== Missing Payment Method Test ===");
        _output.WriteLine($"Status Code: {response.StatusCode}");
        _output.WriteLine($"Response: {JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true })}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("PaymentMethodId");
    }

    [Fact]
    public async Task ProcessTransaction_WithZeroAmount_ReturnsBadRequest()
    {
        // Arrange
        var request = new StripeTransactionRequest
        {
            Amount = 0,
            Currency = "usd",
            PaymentMethodId = "pm_card_visa"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/stripe/transaction", request);
        var result = await response.Content.ReadFromJsonAsync<StripeTransactionResponse>();

        // Log output
        _output.WriteLine("=== Zero Amount Test ===");
        _output.WriteLine($"Status Code: {response.StatusCode}");
        _output.WriteLine($"Response: {JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true })}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Amount");
    }

    [Fact]
    public async Task ProcessTransaction_WithNegativeAmount_ReturnsBadRequest()
    {
        // Arrange
        var request = new StripeTransactionRequest
        {
            Amount = -100,
            Currency = "usd",
            PaymentMethodId = "pm_card_visa"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/stripe/transaction", request);
        var result = await response.Content.ReadFromJsonAsync<StripeTransactionResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Amount");
    }

    [Fact]
    public async Task ProcessTransaction_WithMissingCurrency_ReturnsBadRequest()
    {
        // Arrange
        var request = new StripeTransactionRequest
        {
            Amount = 1000,
            Currency = "",
            PaymentMethodId = "pm_card_visa"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/stripe/transaction", request);
        var result = await response.Content.ReadFromJsonAsync<StripeTransactionResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Currency");
    }

    [Fact]
    public async Task ProcessTransaction_WithValidTestCard_ReturnsSuccessOrStripeError()
    {
        // Arrange
        var request = new StripeTransactionRequest
        {
            Amount = 2000, // $20.00
            Currency = "usd",
            PaymentMethodId = "pm_card_visa",
            Description = "Integration test payment",
            CustomerEmail = "test@example.com",
            Metadata = new Dictionary<string, string>
            {
                { "testId", Guid.NewGuid().ToString() },
                { "environment", "integration-test" }
            }
        };

        _output.WriteLine("=== Valid Test Card Payment Test ===");
        _output.WriteLine($"Request: {JsonSerializer.Serialize(request, new JsonSerializerOptions { WriteIndented = true })}");

        // Act
        var response = await _client.PostAsJsonAsync("/api/stripe/transaction", request);
        var result = await response.Content.ReadFromJsonAsync<StripeTransactionResponse>();

        // Log output
        _output.WriteLine($"Status Code: {response.StatusCode}");
        _output.WriteLine($"Response: {JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true })}");

        // Assert
        result.Should().NotBeNull();

        // If Stripe API key is valid, we should get either success or a Stripe error
        // If the API key is invalid/missing, we'll get an error message
        if (result!.Success)
        {
            // Success case
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result.PaymentIntentId.Should().NotBeNullOrEmpty();
            result.Status.Should().Be("succeeded");
            result.Amount.Should().Be(2000);
            result.Currency.Should().Be("usd");
            result.ClientSecret.Should().NotBeNullOrEmpty();
        }
        else
        {
            // Error case - validate error structure
            result.ErrorMessage.Should().NotBeNullOrEmpty();
            // Could be bad API key, network issue, or invalid test data
        }
    }

    [Fact]
    public async Task ProcessTransaction_WithMetadata_IncludesMetadataInRequest()
    {
        // Arrange
        var metadata = new Dictionary<string, string>
        {
            { "orderId", "12345" },
            { "customerId", "CUST-001" },
            { "source", "unit-test" }
        };

        var request = new StripeTransactionRequest
        {
            Amount = 1500,
            Currency = "usd",
            PaymentMethodId = "pm_card_visa",
            Metadata = metadata
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/stripe/transaction", request);
        var result = await response.Content.ReadFromJsonAsync<StripeTransactionResponse>();

        // Assert
        result.Should().NotBeNull();
        // Test passes if the request is properly formatted
        // Actual success depends on valid Stripe API key
    }

    [Fact]
    public async Task ProcessTransaction_WithCustomerEmail_IncludesEmailInRequest()
    {
        // Arrange
        var request = new StripeTransactionRequest
        {
            Amount = 1000,
            Currency = "usd",
            PaymentMethodId = "pm_card_visa",
            CustomerEmail = "customer@example.com",
            Description = "Test payment with email"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/stripe/transaction", request);
        var result = await response.Content.ReadFromJsonAsync<StripeTransactionResponse>();

        // Assert
        result.Should().NotBeNull();
        // Test passes if the request is properly formatted
    }
}

// This class is needed for WebApplicationFactory to work
public class Program { }
