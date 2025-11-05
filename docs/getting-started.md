# Getting Started with SimpleStripeApi

Complete guide for integrating Stripe payments into your .NET 8+ application.

## Table of Contents

- [Prerequisites](#prerequisites)
- [Installation](#installation)
- [Configuration](#configuration)
- [Basic Integration](#basic-integration)
- [Making Your First Transaction](#making-your-first-transaction)
- [Testing](#testing)
- [Next Steps](#next-steps)

## Prerequisites

Before you begin, ensure you have:

- **.NET 8.0 SDK or later** - [Download here](https://dotnet.microsoft.com/download)
- **A Stripe account** - [Sign up for free](https://dashboard.stripe.com/register)
- **Your Stripe test API key** - Available in your [Stripe Dashboard](https://dashboard.stripe.com/test/apikeys)
- **Basic knowledge of ASP.NET Core** and C#

## Installation

### Option 1: Add as Project Reference (Recommended for Development)

1. Clone or download the SimpleStripeApi repository:
```bash
git clone https://github.com/emoa2l/SimpleStripeApi.git
```

2. Add the project reference to your application:
```bash
cd YourProject
dotnet add reference ../SimpleStripeApi/SimpleStripeApi.csproj
```

### Option 2: Copy Source Files

Copy these folders into your project:
- `Models/`
- `Services/`
- `Extensions/`

### Option 3: NuGet Package (Coming Soon)

```bash
dotnet add package SimpleStripeApi
```

## Configuration

### Step 1: Get Your Stripe API Key

1. Log in to your [Stripe Dashboard](https://dashboard.stripe.com)
2. Navigate to **Developers** → **API Keys**
3. Copy your **Test mode Secret key** (starts with `sk_test_`)

⚠️ **Important**: Never commit API keys to source control!

### Step 2: Configure Your API Key

Choose one of these methods:

**Method A: Environment Variable (Recommended for Production)**

```bash
# Linux/macOS
export Stripe__SecretKey="sk_test_your_key_here"

# Windows PowerShell
$env:Stripe__SecretKey="sk_test_your_key_here"

# Windows Command Prompt
set Stripe__SecretKey=sk_test_your_key_here
```

**Method B: appsettings.json (For Local Development)**

Create or edit `appsettings.json`:

```json
{
  "Stripe": {
    "SecretKey": "sk_test_your_key_here"
  }
}
```

Add to `.gitignore`:
```
**/appsettings.json
!**/appsettings.example.json
```

## Basic Integration

### Step 1: Register Services

Open your `Program.cs` and add these lines:

```csharp
using SimpleStripeApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add your other services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add SimpleStripeApi service
builder.Services.AddStripeTransactionService();

var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

// Register Stripe endpoints (default route: /api/stripe/transaction)
app.MapStripeTransactionEndpoints();

app.Run();
```

### Step 2: Run Your Application

```bash
dotnet run
```

Your Stripe payment endpoint is now live at:
- `http://localhost:5000/api/stripe/transaction`

## Making Your First Transaction

### Using cURL

```bash
curl -X POST http://localhost:5000/api/stripe/transaction \
  -H "Content-Type: application/json" \
  -d '{
    "amount": 1000,
    "currency": "usd",
    "paymentMethodId": "pm_card_visa",
    "description": "Test payment",
    "customerEmail": "customer@example.com"
  }'
```

### Using C# HttpClient

```csharp
using System.Net.Http.Json;

var client = new HttpClient();
var request = new
{
    Amount = 1000,  // $10.00 in cents
    Currency = "usd",
    PaymentMethodId = "pm_card_visa",
    Description = "Test payment",
    CustomerEmail = "customer@example.com",
    Metadata = new Dictionary<string, string>
    {
        { "orderId", "12345" }
    }
};

var response = await client.PostAsJsonAsync(
    "http://localhost:5000/api/stripe/transaction",
    request
);

var result = await response.Content.ReadAsStringAsync();
Console.WriteLine(result);
```

### Using JavaScript/Fetch

```javascript
const response = await fetch('http://localhost:5000/api/stripe/transaction', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
  },
  body: JSON.stringify({
    amount: 1000,
    currency: 'usd',
    paymentMethodId: 'pm_card_visa',
    description: 'Test payment',
    customerEmail: 'customer@example.com'
  })
});

const result = await response.json();
console.log(result);
```

### Expected Response

**Success (200 OK):**
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

**Error (400 Bad Request):**
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

## Testing

### Test Payment Methods

Stripe provides test payment method IDs you can use:

| Payment Method | Result |
|----------------|--------|
| `pm_card_visa` | ✅ Successful payment |
| `pm_card_mastercard` | ✅ Successful payment |
| `pm_card_amex` | ✅ Successful payment |
| `pm_card_chargeDeclined` | ❌ Card declined |
| `pm_card_chargeDeclinedInsufficientFunds` | ❌ Insufficient funds |

[More test cards →](https://stripe.com/docs/testing)

### View Test Payments

1. Go to your [Stripe Dashboard](https://dashboard.stripe.com)
2. Ensure you're in **Test Mode** (toggle in top right)
3. Click **Payments** to see all test transactions
4. Click any payment to view details including metadata

## Next Steps

### Add Authentication

Protect your payment endpoint with authentication:

```csharp
app.MapStripeTransactionEndpoints()
    .RequireAuthorization();
```

### Add Rate Limiting

Prevent abuse with rate limiting:

```csharp
app.MapStripeTransactionEndpoints()
    .RequireRateLimiting("fixed");
```

### Custom Route Prefix

Change the endpoint path:

```csharp
// Default: /api/stripe/transaction
app.MapStripeTransactionEndpoints();

// Custom: /payments/stripe/transaction
app.MapStripeTransactionEndpoints("payments/stripe");

// Root level: /transaction
app.MapStripeTransactionEndpoints("");
```

### Production Deployment

When deploying to production:

1. **Switch to live API key** (starts with `sk_live_`)
2. **Enable HTTPS** - Add `app.UseHttpsRedirection()`
3. **Add authentication** - Use JWT or OAuth
4. **Implement rate limiting** - Prevent API abuse
5. **Add logging** - Monitor transactions
6. **Set up webhooks** - Handle async events from Stripe

### Learn More

- [API Reference](./api-reference.md)
- [Main README](../README.md)
- [Testing Guide](../TESTING.md)
- [Stripe API Documentation](https://stripe.com/docs/api)

## Troubleshooting

### "Stripe:SecretKey configuration is missing"

**Solution**: Ensure your API key is configured via environment variable or appsettings.json.

### "Invalid API Key provided"

**Solution**:
- Verify you copied the complete key
- Ensure it starts with `sk_test_` for test mode
- Check for extra spaces or newlines

### Network Errors

**Solution**:
- Check internet connection
- Verify api.stripe.com is accessible
- Check firewall/proxy settings

## Support

- **Issues**: [GitHub Issues](https://github.com/emoa2l/SimpleStripeApi/issues)
- **Stripe Docs**: [stripe.com/docs](https://stripe.com/docs)
- **Stripe Support**: [support.stripe.com](https://support.stripe.com)
