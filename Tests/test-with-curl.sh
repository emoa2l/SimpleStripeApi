#!/bin/bash

# Simple Stripe API Test Script
# This script tests various scenarios against the Stripe API

BASE_URL="http://localhost:5000"
API_ENDPOINT="${BASE_URL}/api/stripe/transaction"

echo "========================================="
echo "Simple Stripe API Test Suite"
echo "========================================="
echo ""

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Test 1: Health Check
echo "Test 1: Health Check"
echo "---"
response=$(curl -s -w "\n%{http_code}" "${BASE_URL}/health")
http_code=$(echo "$response" | tail -n1)
body=$(echo "$response" | head -n-1)

if [ "$http_code" == "200" ]; then
    echo -e "${GREEN}✓ PASSED${NC} - Health check returned 200"
    echo "Response: $body"
else
    echo -e "${RED}✗ FAILED${NC} - Health check returned $http_code"
fi
echo ""

# Test 2: Successful Payment
echo "Test 2: Successful Payment (Test Card)"
echo "---"
response=$(curl -s -w "\n%{http_code}" -X POST "$API_ENDPOINT" \
  -H "Content-Type: application/json" \
  -d '{
    "amount": 2000,
    "currency": "usd",
    "paymentMethodId": "pm_card_visa",
    "description": "Test payment - curl script",
    "customerEmail": "test@example.com",
    "metadata": {
      "testId": "curl-test-1"
    }
  }')
http_code=$(echo "$response" | tail -n1)
body=$(echo "$response" | head -n-1)

if [ "$http_code" == "200" ] || [ "$http_code" == "400" ]; then
    echo -e "${YELLOW}⚠ CHECK${NC} - Payment returned $http_code (depends on API key validity)"
    echo "Response: $body"
else
    echo -e "${RED}✗ FAILED${NC} - Payment returned unexpected code $http_code"
fi
echo ""

# Test 3: Missing Payment Method ID
echo "Test 3: Validation - Missing Payment Method ID"
echo "---"
response=$(curl -s -w "\n%{http_code}" -X POST "$API_ENDPOINT" \
  -H "Content-Type: application/json" \
  -d '{
    "amount": 1000,
    "currency": "usd",
    "paymentMethodId": "",
    "description": "Should fail validation"
  }')
http_code=$(echo "$response" | tail -n1)
body=$(echo "$response" | head -n-1)

if [ "$http_code" == "400" ]; then
    echo -e "${GREEN}✓ PASSED${NC} - Validation correctly rejected empty PaymentMethodId"
    echo "Response: $body"
else
    echo -e "${RED}✗ FAILED${NC} - Expected 400, got $http_code"
fi
echo ""

# Test 4: Zero Amount
echo "Test 4: Validation - Zero Amount"
echo "---"
response=$(curl -s -w "\n%{http_code}" -X POST "$API_ENDPOINT" \
  -H "Content-Type: application/json" \
  -d '{
    "amount": 0,
    "currency": "usd",
    "paymentMethodId": "pm_card_visa"
  }')
http_code=$(echo "$response" | tail -n1)
body=$(echo "$response" | head -n-1)

if [ "$http_code" == "400" ]; then
    echo -e "${GREEN}✓ PASSED${NC} - Validation correctly rejected zero amount"
    echo "Response: $body"
else
    echo -e "${RED}✗ FAILED${NC} - Expected 400, got $http_code"
fi
echo ""

# Test 5: Negative Amount
echo "Test 5: Validation - Negative Amount"
echo "---"
response=$(curl -s -w "\n%{http_code}" -X POST "$API_ENDPOINT" \
  -H "Content-Type: application/json" \
  -d '{
    "amount": -100,
    "currency": "usd",
    "paymentMethodId": "pm_card_visa"
  }')
http_code=$(echo "$response" | tail -n1)
body=$(echo "$response" | head -n-1)

if [ "$http_code" == "400" ]; then
    echo -e "${GREEN}✓ PASSED${NC} - Validation correctly rejected negative amount"
    echo "Response: $body"
else
    echo -e "${RED}✗ FAILED${NC} - Expected 400, got $http_code"
fi
echo ""

# Test 6: Missing Currency
echo "Test 6: Validation - Missing Currency"
echo "---"
response=$(curl -s -w "\n%{http_code}" -X POST "$API_ENDPOINT" \
  -H "Content-Type: application/json" \
  -d '{
    "amount": 1000,
    "currency": "",
    "paymentMethodId": "pm_card_visa"
  }')
http_code=$(echo "$response" | tail -n1)
body=$(echo "$response" | head -n-1)

if [ "$http_code" == "400" ]; then
    echo -e "${GREEN}✓ PASSED${NC} - Validation correctly rejected empty currency"
    echo "Response: $body"
else
    echo -e "${RED}✗ FAILED${NC} - Expected 400, got $http_code"
fi
echo ""

# Test 7: With Metadata
echo "Test 7: Payment with Metadata"
echo "---"
response=$(curl -s -w "\n%{http_code}" -X POST "$API_ENDPOINT" \
  -H "Content-Type: application/json" \
  -d '{
    "amount": 1500,
    "currency": "usd",
    "paymentMethodId": "pm_card_visa",
    "description": "Payment with metadata",
    "customerEmail": "metadata@example.com",
    "metadata": {
      "orderId": "ORDER-12345",
      "customerId": "CUST-001",
      "source": "curl-test"
    }
  }')
http_code=$(echo "$response" | tail -n1)
body=$(echo "$response" | head -n-1)

if [ "$http_code" == "200" ] || [ "$http_code" == "400" ]; then
    echo -e "${YELLOW}⚠ CHECK${NC} - Payment with metadata returned $http_code"
    echo "Response: $body"
else
    echo -e "${RED}✗ FAILED${NC} - Payment returned unexpected code $http_code"
fi
echo ""

echo "========================================="
echo "Test Suite Complete"
echo "========================================="
echo ""
echo "Note: Tests that check against actual Stripe API (Test 2, 7) may"
echo "return 400 if using an invalid/placeholder API key. This is expected."
echo "To test with real Stripe API, set a valid test key in appsettings.json"
