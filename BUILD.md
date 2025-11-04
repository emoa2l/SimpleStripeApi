# Build Instructions

## Building the Library Only

To build just the SimpleStripeApi library without tests:

```bash
dotnet build SimpleStripeApi.csproj
```

## Building Everything (Recommended)

To build the library, test API, and tests together:

```bash
dotnet build SimpleStripeApi.sln
```

Or build from the solution directory:

```bash
dotnet build
```

## Building Individual Projects

```bash
# Build just the library
dotnet build SimpleStripeApi.csproj

# Build the test API
dotnet build TestApi/TestApi.csproj

# Build the tests
dotnet build Tests/Tests.csproj
```

## Running Tests

```bash
# Run all tests
dotnet test

# Or specify the test project
dotnet test Tests/Tests.csproj
```

## Running the Test API

```bash
cd TestApi
dotnet run
```

Then visit http://localhost:5000/swagger

## Project Structure

```
SimpleStripeApi/
├── SimpleStripeApi.csproj      # Main library (this is what you reference)
├── SimpleStripeApi.sln         # Solution file (builds everything)
├── Models/                     # Request/Response DTOs
├── Services/                   # Stripe transaction service
├── Extensions/                 # Self-registration extensions
├── TestApi/                    # Sample application (separate project)
│   └── TestApi.csproj
└── Tests/                      # Integration tests (separate project)
    └── Tests.csproj
```

## Integration into Your Project

When integrating SimpleStripeApi into your existing .NET project:

**Option 1: Add as Project Reference**

```bash
dotnet add YourProject.csproj reference path/to/SimpleStripeApi/SimpleStripeApi.csproj
```

**Option 2: Copy the Source Files**

Copy only these folders to your project:
- `Models/`
- `Services/`
- `Extensions/`

The `Tests/` and `TestApi/` folders are optional and only needed for development/testing.

## Common Build Issues

### Issue: Tests are being compiled with the main project

**Solution:** Make sure you're building the solution file (`SimpleStripeApi.sln`) or the individual project files, not trying to compile everything together.

### Issue: Missing dependencies

**Solution:** Restore NuGet packages first:

```bash
dotnet restore
dotnet build
```

### Issue: .NET SDK not found

**Solution:** Install .NET 8.0 SDK from https://dotnet.microsoft.com/download

Check your version:
```bash
dotnet --version
```

Should be 8.0.x or higher.
