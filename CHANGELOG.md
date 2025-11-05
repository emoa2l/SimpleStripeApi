# Changelog

All notable changes to SimpleStripeApi will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Comprehensive route configuration tests
- GitHub Sponsors funding configuration
- Detailed API reference documentation
- Getting started guide

### Changed
- Updated README with environment variable configuration instructions
- Improved test infrastructure to support environment variables

### Fixed
- Test infrastructure now properly reads STRIPE_SECRET_KEY environment variable
- Removed duplicate Program class declaration in tests

## [1.0.0] - 2024-11-04

### Added
- Initial release of SimpleStripeApi
- Async transaction processing via POST /api/stripe/transaction endpoint
- JSON-based API with request/response models
- Support for payment amount, currency, payment method ID
- Optional description, customer email, and metadata fields
- Built on official Stripe.net SDK (v45.9.0)
- Self-registering service extension: `AddStripeTransactionService()`
- Self-registering route extension: `MapStripeTransactionEndpoints()`
- Customizable route prefix (default: "api/stripe")
- Method chaining support via RouteGroupBuilder return type
- Automatic PaymentIntent creation and confirmation
- Comprehensive error handling with descriptive messages
- Request validation (amount > 0, required fields)
- XUnit integration tests using WebApplicationFactory
- Test utilities including curl script and HTTP file samples
- Sample TestApi application demonstrating integration
- Swagger/OpenAPI support
- MIT License

### Configuration
- Configuration via `Stripe:SecretKey` in appsettings.json or environment variable
- Supports .NET 8.0 and later
- Minimal dependencies design
- No HTTPS enforcement (delegated to host application)
- No built-in authentication (add via .RequireAuthorization())
- No built-in rate limiting (add via .RequireRateLimiting())

### Documentation
- Comprehensive README with quick start guide
- BUILD.md with build instructions
- TESTING.md with testing strategies
- Tests/README.md with detailed test documentation
- appsettings.example.json configuration template
- Code examples in C#, JavaScript, and curl
- OpenAPI/Swagger integration for interactive testing

### Testing
- 8 XUnit integration tests covering:
  - Health check endpoint
  - Request validation (missing fields, zero/negative amounts)
  - Successful payments with test cards
  - Metadata and customer email support
  - Stripe API integration
- Automated bash script for curl-based testing
- HTTP file for IDE-based testing (VS Code, Rider)
- WebApplicationFactory test setup
- FluentAssertions for readable test assertions

### Architecture
- Class library targeting .NET 8.0
- Three-namespace structure: Models, Services, Extensions
- Dependency injection ready
- IConfiguration integration
- ASP.NET Core Minimal API endpoints
- RouteGroupBuilder for flexible configuration
- Singleton service lifetime for efficiency

## Release Notes

### Version 1.0.0

SimpleStripeApi 1.0.0 is the initial stable release, providing a minimal, drop-in solution for Stripe payment processing in .NET 8+ applications. This release focuses on simplicity and ease of integration while maintaining flexibility through extension points.

**Key Highlights:**

- **Two-line integration**: Add one service registration and one endpoint mapping
- **JSON-first API**: Clean REST interface with strongly-typed models
- **Async by default**: Built for modern async/await patterns
- **Test-friendly**: Comprehensive test suite and utilities included
- **Production ready**: Built on official Stripe.net SDK
- **Extensible**: Returns RouteGroupBuilder for authentication, rate limiting, etc.

**Target Audience:**

Ideal for developers building:
- E-commerce checkout systems
- SaaS subscription platforms
- Donation/payment processing features
- Any .NET application requiring Stripe integration

**Breaking Changes:**

None (initial release)

**Upgrade Notes:**

None (initial release)

**Known Limitations:**

- Single transaction per request (batch processing not supported)
- No webhook handling (planned for future release)
- No refund processing (planned for future release)
- No subscription management (planned for future release)
- No customer management beyond email receipt
- No 3D Secure redirect support (API-only mode)

**Dependencies:**

- .NET 8.0+
- Stripe.net v45.9.0+
- Microsoft.AspNetCore.App (framework reference)
- Microsoft.AspNetCore.OpenApi v8.0.0+

**Tested On:**

- .NET 8.0.21
- macOS (Darwin 25.1.0)
- With Stripe test mode API

**What's Next:**

See [Roadmap in README](README.md#roadmap) for planned features including:
- Webhook handling
- Refund processing
- Subscription support
- Customer management

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines on:
- Reporting bugs
- Suggesting enhancements
- Submitting pull requests
- Code style and conventions

## Support

- **Issues**: [GitHub Issues](https://github.com/emoa2l/SimpleStripeApi/issues)
- **Discussions**: [GitHub Discussions](https://github.com/emoa2l/SimpleStripeApi/discussions)
- **Stripe Docs**: [stripe.com/docs](https://stripe.com/docs)

---

[Unreleased]: https://github.com/emoa2l/SimpleStripeApi/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/emoa2l/SimpleStripeApi/releases/tag/v1.0.0
