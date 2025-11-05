# SimpleStripeApi - Drop-in Stripe Payment Processing for .NET 8+

A simple, drop-in Stripe payment processing API for .NET 8+ projects. This library provides a self-registering route for handling Stripe transactions asynchronously with JSON input/output.

## Features

- ✅ Single transaction per request (async)
- ✅ JSON-only input and output
- ✅ Self-registering routes via extension methods
- ✅ Built on Stripe.net SDK
- ✅ Minimal configuration required
- ✅ Easy to drop into existing .NET projects

## Installation

### Option 1: Add as Project Reference

1. Copy this folder into your solution
2. Add project reference to your main project:

```bash
dotnet add reference path/to/SimpleStripeApi/SimpleStripeApi.csproj
```

### Option 2: Add as NuGet Package Source

Build and add to your local NuGet feed or private NuGet repository.

## Quick Start

### 1. Add Configuration

Configure your Stripe secret key using one of these methods:

**Option A: Environment Variable (Recommended for production)**

```bash
# ASP.NET Core uses double underscores for hierarchical configuration
export Stripe__SecretKey="sk_test_your_secret_key_here"
```

Or set it in your hosting environment (Azure App Service, Docker, Kubernetes, etc.):
- Azure App Service: Add `Stripe__SecretKey` in Configuration → Application settings
- Docker: Use `-e Stripe__SecretKey=sk_test_...`
- Kubernetes: Set in ConfigMap or Secret

**Option B: appsettings.json (For local development)**

```json
{
  "Stripe": {
    "SecretKey": "sk_test_your_secret_key_here"
  }
}
```

**Important:** Never commit your secret key to source control. The library reads from `IConfiguration`, which automatically supports both appsettings.json and environment variables.

### 2. Register Services and Routes

In your `Program.cs`:

```csharp
using SimpleStripeApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add Stripe transaction service
builder.Services.AddStripeTransactionService();

var app = builder.Build();

// Register Stripe routes (default: /api/stripe/transaction)
app.MapStripeTransactionEndpoints();

// Or with custom prefix:
// app.MapStripeTransactionEndpoints("payments/stripe");

app.Run();
```

### 3. Start Processing Payments

That's it! Your API is ready to process transactions.

## API Endpoint

### POST /api/stripe/transaction

Process a single Stripe payment transaction.

**Request Body:**

```json
{
  "amount": 1000,
  "currency": "usd",
  "paymentMethodId": "pm_card_visa",
  "description": "Test payment",
  "customerEmail": "customer@example.com",
  "metadata": {
    "orderId": "12345",
    "customField": "value"
  }
}
```

**Request Fields:**

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `amount` | long | Yes | Amount in cents (e.g., 1000 = $10.00) |
| `currency` | string | Yes | Three-letter ISO currency code (e.g., "usd") |
| `paymentMethodId` | string | Yes | Payment method ID from Stripe.js |
| `description` | string | No | Description for the transaction |
| `customerEmail` | string | No | Customer email for receipt |
| `metadata` | object | No | Custom key-value pairs |

**Success Response (200 OK):**

```json
{
  "success": true,
  "paymentIntentId": "pi_3AbCdEfGhIjKlMnO",
  "status": "succeeded",
  "amount": 1000,
  "currency": "usd",
  "clientSecret": "pi_3AbCdEfGhIjKlMnO_secret_XYZ"
}
```

**Error Response (400 Bad Request):**

```json
{
  "success": false,
  "errorMessage": "Your card was declined.",
  "paymentIntentId": null,
  "status": null,
  "amount": null,
  "currency": null,
  "clientSecret": null
}
```

## Example Usage

### Using cURL

```bash
curl -X POST http://localhost:5000/api/stripe/transaction \
  -H "Content-Type: application/json" \
  -d '{
    "amount": 2500,
    "currency": "usd",
    "paymentMethodId": "pm_card_visa",
    "description": "Order #12345",
    "customerEmail": "customer@example.com"
  }'
```

### Using C# HttpClient

```csharp
using System.Net.Http.Json;

var client = new HttpClient();
var request = new StripeTransactionRequest
{
    Amount = 2500,
    Currency = "usd",
    PaymentMethodId = "pm_card_visa",
    Description = "Order #12345",
    CustomerEmail = "customer@example.com"
};

var response = await client.PostAsJsonAsync(
    "http://localhost:5000/api/stripe/transaction",
    request
);

var result = await response.Content.ReadFromJsonAsync<StripeTransactionResponse>();
```

### Using JavaScript/Fetch

```javascript
const response = await fetch('http://localhost:5000/api/stripe/transaction', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
  },
  body: JSON.stringify({
    amount: 2500,
    currency: 'usd',
    paymentMethodId: 'pm_card_visa',
    description: 'Order #12345',
    customerEmail: 'customer@example.com'
  })
});

const result = await response.json();
console.log(result);
```

## Configuration Options

### Custom Route Prefix

```csharp
// Default route: /api/stripe/transaction
app.MapStripeTransactionEndpoints();

// Custom route: /payments/stripe/transaction
app.MapStripeTransactionEndpoints("payments/stripe");

// Root level: /transaction
app.MapStripeTransactionEndpoints("");
```

### Additional Route Configuration

```csharp
app.MapStripeTransactionEndpoints()
    .RequireAuthorization()           // Add authentication
    .RequireRateLimiting("fixed");    // Add rate limiting
```

## Security Considerations

1. **Never expose your Stripe Secret Key** - Use environment variables or Azure Key Vault
2. **Use HTTPS in Production** - This library does not enforce HTTPS redirection. Configure SSL/TLS in your host application (e.g., via reverse proxy, Azure App Service, or `app.UseHttpsRedirection()`)
3. **Add Authentication** - Protect your endpoints with authentication
4. **Validate Input** - The library includes basic validation, but add your own business rules
5. **Rate Limiting** - Implement rate limiting to prevent abuse
6. **Logging** - Add logging for audit trails and debugging

**Note:** This is a minimal drop-in library. HTTPS, authentication, and rate limiting should be configured in your host application.

## Testing

You can test with Stripe's test payment methods:

- Successful payment: `pm_card_visa`
- Card declined: `pm_card_chargeDeclined`
- Insufficient funds: `pm_card_chargeDeclinedInsufficientFunds`

See [Stripe's testing documentation](https://stripe.com/docs/testing) for more test cards.

## Dependencies

- .NET 8.0+
- Stripe.net (v45.9.0+)
- Microsoft.AspNetCore.Http.Abstractions
- Microsoft.Extensions.Configuration.Abstractions
- Microsoft.Extensions.DependencyInjection.Abstractions

## License

MIT License - See LICENSE file for details

## Support

For issues or questions:
- Stripe API Documentation: https://stripe.com/docs/api
- Stripe.net SDK: https://github.com/stripe/stripe-dotnet

## Roadmap

Future enhancements:
- Support for payment intents with setup for future use
- Webhook handling for async payment confirmations
- Support for subscriptions
- Refund processing
- Customer management
