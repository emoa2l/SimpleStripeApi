# Testing SimpleStripeApi

This document provides a comprehensive guide for testing the SimpleStripeApi library.

## Quick Start Testing

### 1. Get a Stripe Test API Key

1. Go to https://dashboard.stripe.com/register
2. Sign up for a free account
3. Navigate to **Developers** → **API Keys**
4. Copy your **Test mode Secret key** (starts with `sk_test_`)

### 2. Set Your API Key

```bash
# Export as environment variable
export STRIPE_SECRET_KEY="sk_test_your_actual_key_here"
```

Or edit `TestApi/appsettings.json`:
```json
{
  "Stripe": {
    "SecretKey": "sk_test_your_actual_key_here"
  }
}
```

### 3. Run Tests

```bash
# Build everything
dotnet build

# Run unit/integration tests
dotnet test

# Or run the test API and test manually
cd TestApi
dotnet run

# In another terminal
cd Tests
./test-with-curl.sh
```

## Testing Methods

### Method 1: Automated Integration Tests (Recommended)

Uses XUnit with WebApplicationFactory to test the entire stack:

```bash
cd Tests
dotnet test --logger "console;verbosity=detailed"
```

**Tests include:**
- Request validation (no API key needed)
- End-to-end payment processing
- Error handling
- Metadata support

### Method 2: Bash Script Testing

Automated curl-based tests with colored output:

```bash
# Start the API
cd TestApi && dotnet run &

# Run tests
cd Tests
./test-with-curl.sh
```

### Method 3: Manual Testing with HTTP Files

Use VS Code REST Client or JetBrains Rider:

1. Open `Tests/sample-requests.http`
2. Start TestApi: `cd TestApi && dotnet run`
3. Click "Send Request" above each test case

### Method 4: Manual curl Testing

```bash
# Health check
curl http://localhost:5000/health

# Successful payment
curl -X POST http://localhost:5000/api/stripe/transaction \
  -H "Content-Type: application/json" \
  -d '{
    "amount": 2000,
    "currency": "usd",
    "paymentMethodId": "pm_card_visa",
    "description": "Test payment"
  }'

# Validation error
curl -X POST http://localhost:5000/api/stripe/transaction \
  -H "Content-Type: application/json" \
  -d '{
    "amount": 0,
    "currency": "usd",
    "paymentMethodId": "pm_card_visa"
  }'
```

### Method 5: Swagger UI Testing

Most user-friendly for manual testing:

```bash
cd TestApi
dotnet run
```

Then open your browser to: http://localhost:5000/swagger

You can interactively test all endpoints with a nice UI.

## Test Scenarios

### Validation Tests (Work without valid API key)

| Test | Expected Result |
|------|----------------|
| Zero amount | 400 Bad Request - "Amount must be greater than zero" |
| Negative amount | 400 Bad Request - "Amount must be greater than zero" |
| Empty payment method | 400 Bad Request - "PaymentMethodId is required" |
| Empty currency | 400 Bad Request - "Currency is required" |

### Stripe API Tests (Require valid API key)

| Test Card | Expected Result |
|-----------|----------------|
| `pm_card_visa` | 200 OK - Success with payment intent ID |
| `pm_card_mastercard` | 200 OK - Success |
| `pm_card_chargeDeclined` | 400 Bad Request - "Your card was declined" |
| `pm_card_chargeDeclinedInsufficientFunds` | 400 Bad Request - "Insufficient funds" |

### Additional Test Cases

1. **Metadata Support**
   - Send custom key-value pairs
   - Verify they're included in Stripe dashboard

2. **Customer Email**
   - Include customer email
   - Stripe sends receipt automatically (in production)

3. **Different Currencies**
   - Test with USD, EUR, GBP, etc.
   - Verify amount is processed correctly

4. **Large Amounts**
   - Test with amounts > $1,000
   - Verify no integer overflow

## Expected Responses

### Success Response (200 OK)

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

### Validation Error (400 Bad Request)

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

### Stripe Error (400 Bad Request)

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

## Viewing Results in Stripe Dashboard

After making test payments:

1. Go to https://dashboard.stripe.com
2. Ensure you're in **Test Mode** (toggle in sidebar)
3. Click **Payments** to see all test transactions
4. Click any payment to see details including metadata

## Testing in Different Environments

### Local Development

```bash
export ASPNETCORE_ENVIRONMENT=Development
export STRIPE_SECRET_KEY="sk_test_..."
dotnet run
```

### Staging/Production

Never use test keys in production! Use environment variables:

```bash
export ASPNETCORE_ENVIRONMENT=Production
export STRIPE_SECRET_KEY="sk_live_..."
dotnet run
```

## Common Issues and Solutions

### "Invalid API Key provided"

**Problem:** API key is not valid or not set correctly

**Solutions:**
- Verify you copied the entire key
- Ensure it starts with `sk_test_` for test mode
- Check for extra spaces or newlines
- Verify the key hasn't been revoked in Stripe dashboard

### "No such payment_method"

**Problem:** Payment method ID doesn't exist

**Solutions:**
- Use Stripe's test payment methods: `pm_card_visa`, etc.
- If using Stripe.js, ensure you created the payment method correctly
- Check that you're in the same mode (test/live) as the payment method

### Connection/Network Errors

**Problem:** Can't reach Stripe API

**Solutions:**
- Check internet connection
- Verify api.stripe.com is accessible
- Check firewall/proxy settings
- Review Stripe status page: https://status.stripe.com

### Tests Pass but Payments Not Showing in Dashboard

**Problem:** Using wrong API key mode

**Solutions:**
- Ensure dashboard is in Test Mode (not Live Mode)
- Verify you're logged into the correct Stripe account
- Check that the API key matches the account

## Performance Testing

For load testing, use a tool like Apache Bench or wrk:

```bash
# Install wrk
# On macOS: brew install wrk
# On Ubuntu: apt-get install wrk

# Create a test payload file
cat > payment.lua << 'EOF'
wrk.method = "POST"
wrk.headers["Content-Type"] = "application/json"
wrk.body = '{"amount":1000,"currency":"usd","paymentMethodId":"pm_card_visa"}'
EOF

# Run load test (10 connections, 30 seconds)
wrk -t2 -c10 -d30s -s payment.lua http://localhost:5000/api/stripe/transaction
```

**Note:** Stripe rate limits test mode requests. Use sparingly.

## Security Testing

### Things to Test

1. **API Key Exposure**
   - Ensure keys are not in logs
   - Verify keys are not in error responses

2. **Input Validation**
   - SQL injection attempts (should be blocked by parameterization)
   - XSS in description fields (JSON encoding handles this)
   - Extremely large numbers (test Int64 limits)

3. **Rate Limiting**
   - Implement rate limiting in production
   - Test with many rapid requests

### Example Security Tests

```bash
# Test with malicious input
curl -X POST http://localhost:5000/api/stripe/transaction \
  -H "Content-Type: application/json" \
  -d '{
    "amount": 1000,
    "currency": "usd",
    "paymentMethodId": "pm_card_visa",
    "description": "<script>alert(\"xss\")</script>"
  }'

# Should handle gracefully without executing script
```

## Continuous Integration

### GitHub Actions

See `.github/workflows/test.yml` for CI/CD setup example.

Key points:
- Use repository secrets for API keys
- Run tests on every push/PR
- Fail build if tests fail

### Azure DevOps

```yaml
trigger:
- main

pool:
  vmImage: 'ubuntu-latest'

steps:
- task: UseDotNet@2
  inputs:
    version: '8.x'

- script: dotnet restore
  displayName: 'Restore dependencies'

- script: dotnet build --no-restore
  displayName: 'Build'

- script: dotnet test --no-build --logger trx
  displayName: 'Run tests'
  env:
    STRIPE_SECRET_KEY: $(StripeTestKey)
```

## Resources

- **Stripe Testing Docs:** https://stripe.com/docs/testing
- **Stripe API Reference:** https://stripe.com/docs/api
- **XUnit Documentation:** https://xunit.net/
- **ASP.NET Core Testing:** https://learn.microsoft.com/aspnet/core/test/integration-tests

## Need Help?

1. Check Stripe status: https://status.stripe.com
2. Review Stripe logs in Dashboard → Developers → Logs
3. Enable detailed logging in your application
4. Check this repository's Issues for known problems
