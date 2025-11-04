# SimpleStripeApi Tests

This directory contains comprehensive tests and sample requests for the SimpleStripeApi library.

## Contents

- **StripeTransactionEndpointTests.cs** - XUnit integration tests
- **sample-requests.http** - HTTP file for testing with REST clients (VS Code, Rider, etc.)
- **test-with-curl.sh** - Bash script for automated testing with curl
- **Tests.csproj** - Test project configuration

## Prerequisites

### Required

- .NET 8.0 SDK or later
- A Stripe test API key (starts with `sk_test_`)

### Optional (for different testing methods)

- VS Code with REST Client extension
- JetBrains Rider
- curl (usually pre-installed on Linux/Mac)

## Setting Up Your Stripe Test API Key

1. Sign up for a Stripe account at https://stripe.com
2. Navigate to Developers → API Keys in your Stripe Dashboard
3. Copy your "Secret key" (test mode) - it starts with `sk_test_`
4. Set it in one of two ways:

### Option 1: Environment Variable (Recommended)

```bash
export STRIPE_SECRET_KEY="sk_test_your_actual_key_here"
```

### Option 2: Configuration File

Edit `TestApi/appsettings.json`:

```json
{
  "Stripe": {
    "SecretKey": "sk_test_your_actual_key_here"
  }
}
```

**⚠️ WARNING: Never commit real API keys to source control!**

## Running the Tests

### Method 1: Run XUnit Integration Tests

```bash
# From the SimpleStripeApi root directory
cd Tests
dotnet test

# Or run from root
dotnet test Tests/Tests.csproj
```

### Method 2: Run the Test API with curl Script

Terminal 1 - Start the API:
```bash
cd TestApi
dotnet run
```

Terminal 2 - Run the test script:
```bash
cd Tests
./test-with-curl.sh
```

### Method 3: Manual Testing with HTTP File

1. Open `Tests/sample-requests.http` in VS Code or Rider
2. Start the TestApi: `cd TestApi && dotnet run`
3. Click the "Send Request" link above each request in the HTTP file

### Method 4: Manual Testing with curl

Start the API:
```bash
cd TestApi
dotnet run
```

Test successful payment:
```bash
curl -X POST http://localhost:5000/api/stripe/transaction \
  -H "Content-Type: application/json" \
  -d '{
    "amount": 2000,
    "currency": "usd",
    "paymentMethodId": "pm_card_visa",
    "description": "Test payment"
  }'
```

## Test Scenarios Covered

### Validation Tests (No Stripe API key required)

These tests validate request structure and don't require a valid Stripe API key:

1. ✅ Missing payment method ID → Should return 400
2. ✅ Zero amount → Should return 400
3. ✅ Negative amount → Should return 400
4. ✅ Missing/empty currency → Should return 400

### Stripe API Integration Tests (Requires valid API key)

These tests make actual calls to the Stripe test API:

1. ✅ Successful payment with `pm_card_visa`
2. ✅ Declined card with `pm_card_chargeDeclined`
3. ✅ Insufficient funds with `pm_card_chargeDeclinedInsufficientFunds`
4. ✅ Payment with metadata
5. ✅ Payment with customer email
6. ✅ Different currency (EUR)
7. ✅ Large payment amounts

## Stripe Test Payment Methods

Stripe provides test payment method IDs you can use without setting up actual cards:

| Payment Method ID | Result |
|-------------------|--------|
| `pm_card_visa` | Successful payment |
| `pm_card_mastercard` | Successful payment |
| `pm_card_amex` | Successful payment |
| `pm_card_chargeDeclined` | Card declined |
| `pm_card_chargeDeclinedInsufficientFunds` | Insufficient funds |
| `pm_card_chargeDeclinedExpiredCard` | Expired card |

For more test cards, see: https://stripe.com/docs/testing

## Expected Test Results

### With Valid Stripe API Key

```
✓ All validation tests should PASS
✓ Health check should PASS
✓ Successful payment tests should return 200 OK with success=true
✓ Declined payment tests should return 400 with success=false
```

### Without Valid Stripe API Key (or with placeholder key)

```
✓ All validation tests should PASS
✓ Health check should PASS
⚠ Payment tests will return 400 with Stripe authentication error
```

This is expected behavior! The validation works regardless of API key validity.

## Test Output Examples

### Successful Payment Response

```json
{
  "success": true,
  "paymentIntentId": "pi_3AbCdEfGhIjKlMnO",
  "status": "succeeded",
  "amount": 2000,
  "currency": "usd",
  "clientSecret": "pi_3AbCdEfGhIjKlMnO_secret_XYZ"
}
```

### Failed Payment Response

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

### Validation Error Response

```json
{
  "success": false,
  "errorMessage": "Amount must be greater than zero",
  "paymentIntentId": null,
  "status": null,
  "amount": null,
  "currency": null,
  "clientSecret": null
}
```

## Running Tests in CI/CD

### GitHub Actions Example

```yaml
name: Test SimpleStripeApi

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'

      - name: Restore dependencies
        run: dotnet restore

      - name: Build
        run: dotnet build --no-restore

      - name: Test
        env:
          STRIPE_SECRET_KEY: ${{ secrets.STRIPE_TEST_KEY }}
        run: dotnet test --no-build --verbosity normal
```

### Setting Up Secrets

Add your Stripe test key to your repository secrets:
- GitHub: Settings → Secrets → Actions → New repository secret
- Name: `STRIPE_TEST_KEY`
- Value: Your `sk_test_...` key

## Troubleshooting

### Issue: "Stripe:SecretKey configuration is missing"

**Solution:** Ensure you've set the Stripe API key in either:
- Environment variable: `STRIPE_SECRET_KEY`
- Configuration file: `TestApi/appsettings.json`

### Issue: "Invalid API Key provided"

**Solution:** Your API key is not valid. Check:
- You're using a test key (starts with `sk_test_`)
- The key is complete with no extra spaces
- The key is not expired or revoked

### Issue: Tests fail with network errors

**Solution:**
- Check your internet connection
- Verify you can reach api.stripe.com
- Check if you're behind a proxy/firewall

### Issue: "Could not load file or assembly"

**Solution:**
```bash
dotnet restore
dotnet build
```

## More Information

- Stripe API Documentation: https://stripe.com/docs/api
- Stripe Testing Guide: https://stripe.com/docs/testing
- XUnit Documentation: https://xunit.net/
