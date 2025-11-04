using SimpleStripeApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Stripe transaction service
builder.Services.AddStripeTransactionService();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Note: HTTPS redirection is not included - let the host application handle HTTPS
// If you need HTTPS, add app.UseHttpsRedirection() in your main application

// Register Stripe transaction endpoints
app.MapStripeTransactionEndpoints();

// Add a health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
    .WithName("HealthCheck")
    .WithOpenApi();

app.Run();

// Make Program class accessible for testing
public partial class Program { }
