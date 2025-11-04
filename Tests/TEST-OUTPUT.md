# How to View Test Output

This guide shows you how to see console output when running tests in the SimpleStripeApi project.

## Using ITestOutputHelper in Tests

The test file uses `ITestOutputHelper` to log output. This is the XUnit way of writing to the test console.

### Example from StripeTransactionEndpointTests.cs

```csharp
using Xunit.Abstractions;
using System.Text.Json;

public class StripeTransactionEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly ITestOutputHelper _output;

    public StripeTransactionEndpointTests(
        WebApplicationFactory<Program> factory,
        ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task ProcessTransaction_WithValidTestCard_ReturnsSuccessOrStripeError()
    {
        // ... arrange code ...

        var result = await response.Content.ReadFromJsonAsync<StripeTransactionResponse>();

        // Log the result object as JSON
        _output.WriteLine("=== Test Output ===");
        _output.WriteLine($"Status Code: {response.StatusCode}");
        _output.WriteLine($"Response: {JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true })}");

        // ... assertions ...
    }
}
```

## How to Run Tests and See Output

### Method 1: Visual Studio (Windows)

1. Open Test Explorer (Test → Test Explorer)
2. Run tests
3. Click on a test
4. View the "Test Detail Summary" pane
5. Output appears under "Standard Output" or "Test Output"

### Method 2: Visual Studio Code

1. Install the "C# Dev Kit" extension
2. Open the Testing panel (beaker icon in sidebar)
3. Run a test
4. Click "Show Output" in the test results

### Method 3: Command Line (Detailed Output)

```bash
# Run with detailed logging
dotnet test --logger "console;verbosity=detailed"
```

Output will show:
```
=== Valid Test Card Payment Test ===
Request: {
  "amount": 2000,
  "currency": "usd",
  "paymentMethodId": "pm_card_visa",
  "description": "Integration test payment",
  "customerEmail": "test@example.com"
}
Status Code: OK
Response: {
  "success": true,
  "paymentIntentId": "pi_3AbCdEfGhIjKlMnO",
  "status": "succeeded",
  "amount": 2000,
  "currency": "usd",
  "clientSecret": "pi_3AbCdEfGhIjKlMnO_secret_XYZ"
}
```

### Method 4: JetBrains Rider

1. Run tests from the Unit Tests window
2. Select a test
3. View the "Console" tab in the test results pane
4. Output appears automatically

## Different Verbosity Levels

```bash
# Minimal output (default)
dotnet test

# Normal output
dotnet test --logger "console;verbosity=normal"

# Detailed output (shows test output)
dotnet test --logger "console;verbosity=detailed"

# Diagnostic output (everything)
dotnet test --logger "console;verbosity=diagnostic"
```

## Run Specific Test with Output

```bash
# Run just one test with detailed output
dotnet test --filter "FullyQualifiedName~ProcessTransaction_WithValidTestCard" --logger "console;verbosity=detailed"
```

## Logging Tips

### 1. Log Request and Response

```csharp
_output.WriteLine($"Request: {JsonSerializer.Serialize(request, new JsonSerializerOptions { WriteIndented = true })}");
_output.WriteLine($"Response: {JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true })}");
```

### 2. Log HTTP Details

```csharp
_output.WriteLine($"Status Code: {response.StatusCode}");
_output.WriteLine($"Headers: {response.Headers}");
_output.WriteLine($"Content: {await response.Content.ReadAsStringAsync()}");
```

### 3. Add Section Headers

```csharp
_output.WriteLine("=== Test Name ===");
_output.WriteLine("--- Request ---");
// ... request details ...
_output.WriteLine("--- Response ---");
// ... response details ...
```

### 4. Log Variables

```csharp
_output.WriteLine($"Variable value: {myVariable}");
_output.WriteLine($"Count: {list.Count}");
_output.WriteLine($"Is null: {obj == null}");
```

## Example Output

When you run `dotnet test --logger "console;verbosity=detailed"`, you'll see:

```
Starting test execution, please wait...
A total of 1 test files matched the specified pattern.

Test run for SimpleStripeApi.Tests.dll (.NETCoreApp,Version=v8.0)
Microsoft (R) Test Execution Command Line Tool Version 17.8.0

  Passed ProcessTransaction_WithValidTestCard_ReturnsSuccessOrStripeError [1.2 s]
  Standard Output:
=== Valid Test Card Payment Test ===
Request: {
  "amount": 2000,
  "currency": "usd",
  "paymentMethodId": "pm_card_visa",
  "description": "Integration test payment",
  "customerEmail": "test@example.com",
  "metadata": {
    "testId": "a1b2c3d4-e5f6-7890-abcd-123456789abc",
    "environment": "integration-test"
  }
}
Status Code: OK
Response: {
  "success": true,
  "paymentIntentId": "pi_3AbCdEfGhIjKlMnO",
  "status": "succeeded",
  "amount": 2000,
  "currency": "usd",
  "clientSecret": "pi_3AbCdEfGhIjKlMnO_secret_XYZ"
}

Test Run Successful.
Total tests: 1
     Passed: 1
```

## Continuous Output During Testing

If you want to see output as tests run (not just at the end):

```bash
dotnet test --logger "console;verbosity=detailed" -- RunConfiguration.DefaultTestRunType=realtime
```

## Save Output to File

```bash
# Save output to a log file
dotnet test --logger "console;verbosity=detailed" > test-output.log 2>&1

# View the file
cat test-output.log
```

## Debugging with Output

When debugging tests in Visual Studio or Rider, the output appears in:
- **Visual Studio**: Debug → Windows → Output (select "Tests" from dropdown)
- **Rider**: Run tool window → Console tab

The `ITestOutputHelper` output will show up regardless of whether you're running or debugging!
