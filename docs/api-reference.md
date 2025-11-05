# API Reference - SimpleStripeApi

Complete reference for all endpoints, models, and configuration options.

## Table of Contents

- [Endpoints](#endpoints)
- [Models](#models)
- [Services](#services)
- [Extension Methods](#extension-methods)
- [Configuration](#configuration)
- [Error Handling](#error-handling)

## Endpoints

### POST /api/stripe/transaction

Process a single Stripe payment transaction.

**Default Route**: `/api/stripe/transaction`

**HTTP Method**: `POST`

**Content-Type**: `application/json`

**Authentication**: None (add via `.RequireAuthorization()`)

#### Request Body

```json
{
  "amount": 1000,
  "currency": "usd",
  "paymentMethodId": "pm_card_visa",
  "description": "Optional description",
  "customerEmail": "optional@example.com",
  "metadata": {
    "key1": "value1",
    "key2": "value2"
  }
}
```

#### Request Parameters

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `amount` | `long` | ✅ Yes | Amount in cents (e.g., 1000 = $10.00). Must be > 0 |
| `currency` | `string` | ✅ Yes | Three-letter ISO currency code (e.g., "usd", "eur", "gbp") |
| `paymentMethodId` | `string` | ✅ Yes | Stripe payment method ID from Stripe.js or test method |
| `description` | `string` | ❌ No | Description of the transaction |
| `customerEmail` | `string` | ❌ No | Customer email for receipt (Stripe sends receipt in production) |
| `metadata` | `Dictionary<string, string>` | ❌ No | Custom key-value pairs for your reference |

#### Response

**Success Response (200 OK)**

```json
{
  "success": true,
  "paymentIntentId": "pi_3AbCdEfGhIjKlMnO",
  "status": "succeeded",
  "amount": 1000,
  "currency": "usd",
  "clientSecret": "pi_3AbCdEfGhIjKlMnO_secret_XYZ",
  "errorMessage": null
}
```

**Validation Error Response (400 Bad Request)**

```json
{
  "success": false,
  "paymentIntentId": null,
  "status": null,
  "amount": null,
  "currency": null,
  "clientSecret": null,
  "errorMessage": "Amount must be greater than zero"
}
```

**Stripe API Error Response (400 Bad Request)**

```json
{
  "success": false,
  "paymentIntentId": null,
  "status": null,
  "amount": null,
  "currency": null,
  "clientSecret": null,
  "errorMessage": "Your card was declined."
}
```

#### Response Fields

| Field | Type | Description |
|-------|------|-------------|
| `success` | `bool` | `true` if payment succeeded, `false` otherwise |
| `paymentIntentId` | `string?` | Stripe payment intent ID (null on failure) |
| `status` | `string?` | Stripe payment status (e.g., "succeeded", null on failure) |
| `amount` | `long?` | Amount charged in cents (null on failure) |
| `currency` | `string?` | Currency code (null on failure) |
| `clientSecret` | `string?` | Client secret for additional confirmation (null on failure) |
| `errorMessage` | `string?` | Error message if payment failed (null on success) |

#### HTTP Status Codes

| Code | Description |
|------|-------------|
| `200` | Payment processed successfully |
| `400` | Validation error or payment declined |
| `500` | Internal server error |

#### Example Requests

**cURL**
```bash
curl -X POST http://localhost:5000/api/stripe/transaction \
  -H "Content-Type: application/json" \
  -d '{
    "amount": 2500,
    "currency": "usd",
    "paymentMethodId": "pm_card_visa",
    "description": "Order #12345",
    "customerEmail": "customer@example.com",
    "metadata": {
      "orderId": "12345",
      "customerId": "CUST-001"
    }
  }'
```

**C#**
```csharp
var response = await httpClient.PostAsJsonAsync(
    "/api/stripe/transaction",
    new StripeTransactionRequest
    {
        Amount = 2500,
        Currency = "usd",
        PaymentMethodId = "pm_card_visa",
        Description = "Order #12345",
        CustomerEmail = "customer@example.com",
        Metadata = new Dictionary<string, string>
        {
            { "orderId", "12345" },
            { "customerId", "CUST-001" }
        }
    }
);
```

**JavaScript**
```javascript
const response = await fetch('/api/stripe/transaction', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    amount: 2500,
    currency: 'usd',
    paymentMethodId: 'pm_card_visa',
    description: 'Order #12345',
    customerEmail: 'customer@example.com',
    metadata: {
      orderId: '12345',
      customerId: 'CUST-001'
    }
  })
});
```

## Models

### StripeTransactionRequest

Request model for payment transactions.

**Namespace**: `SimpleStripeApi.Models`

**Properties**:

```csharp
public class StripeTransactionRequest
{
    public long Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string PaymentMethodId { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CustomerEmail { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}
```

### StripeTransactionResponse

Response model for payment transactions.

**Namespace**: `SimpleStripeApi.Models`

**Properties**:

```csharp
public class StripeTransactionResponse
{
    public bool Success { get; set; }
    public string? PaymentIntentId { get; set; }
    public string? Status { get; set; }
    public long? Amount { get; set; }
    public string? Currency { get; set; }
    public string? ClientSecret { get; set; }
    public string? ErrorMessage { get; set; }
}
```

## Services

### IStripeTransactionService

Interface for Stripe transaction processing.

**Namespace**: `SimpleStripeApi.Services`

**Methods**:

```csharp
public interface IStripeTransactionService
{
    Task<StripeTransactionResponse> ProcessTransactionAsync(
        StripeTransactionRequest request,
        CancellationToken cancellationToken = default
    );
}
```

### StripeTransactionService

Implementation of IStripeTransactionService.

**Namespace**: `SimpleStripeApi.Services`

**Constructor**:

```csharp
public StripeTransactionService(IConfiguration configuration)
```

Reads `Stripe:SecretKey` from configuration. Throws `InvalidOperationException` if key is missing.

## Extension Methods

### AddStripeTransactionService

Registers the Stripe transaction service in the DI container.

**Namespace**: `SimpleStripeApi.Extensions`

**Signature**:

```csharp
public static IServiceCollection AddStripeTransactionService(
    this IServiceCollection services
)
```

**Usage**:

```csharp
builder.Services.AddStripeTransactionService();
```

**What it does**:
- Registers `IStripeTransactionService` as a singleton
- Configures Stripe API with your secret key from configuration

### MapStripeTransactionEndpoints

Maps Stripe transaction endpoints to the application.

**Namespace**: `SimpleStripeApi.Extensions`

**Signature**:

```csharp
public static RouteGroupBuilder MapStripeTransactionEndpoints(
    this IEndpointRouteBuilder endpoints,
    string routePrefix = "api/stripe"
)
```

**Parameters**:

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `routePrefix` | `string` | `"api/stripe"` | Route prefix for endpoints |

**Returns**: `RouteGroupBuilder` for additional configuration

**Usage**:

```csharp
// Default route: /api/stripe/transaction
app.MapStripeTransactionEndpoints();

// Custom route: /payments/stripe/transaction
app.MapStripeTransactionEndpoints("payments/stripe");

// Root level: /transaction
app.MapStripeTransactionEndpoints("");

// With additional configuration
app.MapStripeTransactionEndpoints()
    .RequireAuthorization()
    .RequireRateLimiting("fixed");
```

## Configuration

### Stripe:SecretKey

**Type**: `string` (required)

**Description**: Your Stripe API secret key

**Environment Variable**: `Stripe__SecretKey`

**appsettings.json**:
```json
{
  "Stripe": {
    "SecretKey": "sk_test_your_key_here"
  }
}
```

**Environment Variable (Linux/macOS)**:
```bash
export Stripe__SecretKey="sk_test_your_key_here"
```

**Environment Variable (Windows)**:
```powershell
$env:Stripe__SecretKey="sk_test_your_key_here"
```

### Route Configuration

**Default Route**: `/api/stripe/transaction`

**Customization**:
```csharp
app.MapStripeTransactionEndpoints("custom/prefix");
```

### Middleware Configuration

The library does NOT configure:
- HTTPS redirection (add `app.UseHttpsRedirection()`)
- Authentication (add `.RequireAuthorization()`)
- Rate limiting (add `.RequireRateLimiting()`)

These are left to the host application to configure as needed.

## Error Handling

### Validation Errors

Returned as HTTP 400 with error message:

| Error | Message |
|-------|---------|
| Amount ≤ 0 | "Amount must be greater than zero" |
| Missing PaymentMethodId | "PaymentMethodId is required" |
| Missing Currency | "Currency is required" |

### Stripe API Errors

Stripe errors are caught and returned as HTTP 400 with the Stripe error message:

```json
{
  "success": false,
  "errorMessage": "Your card was declined."
}
```

### Exception Handling

All exceptions are caught and converted to error responses. No exceptions are thrown to the caller.

## Payment Intent Options

The service creates Stripe PaymentIntents with these options:

```csharp
var options = new PaymentIntentCreateOptions
{
    Amount = request.Amount,
    Currency = request.Currency,
    PaymentMethod = request.PaymentMethodId,
    Description = request.Description,
    ReceiptEmail = request.CustomerEmail,
    Confirm = true,  // Automatically confirm payment
    AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
    {
        Enabled = true,
        AllowRedirects = "never"  // API-only, no 3DS redirects
    },
    Metadata = request.Metadata
};
```

## OpenAPI/Swagger Integration

Endpoints are automatically documented with OpenAPI metadata:

- Named endpoint: `ProcessStripeTransaction`
- Produces: `200 OK`, `400 Bad Request`, `500 Internal Server Error`
- Tagged as: `Stripe Transactions`

## Testing

See [TESTING.md](../TESTING.md) for:
- Test payment method IDs
- Integration test examples
- WebApplicationFactory testing
- curl script testing

## See Also

- [Getting Started Guide](./getting-started.md)
- [Main README](../README.md)
- [Testing Guide](../TESTING.md)
- [Stripe API Documentation](https://stripe.com/docs/api/payment_intents)
