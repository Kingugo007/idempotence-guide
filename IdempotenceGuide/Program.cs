using IdempotenceGuide.Contract;
using IdempotenceGuide.Middleware;
using IdempotenceGuide.Persistence;
using IdempotenceGuide.Service;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// For development/testing, use in-memory cache
builder.Services.AddMemoryCache();

// Add database context
// Add database context using EF Core In-Memory provider
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("IdempotenceExampleDb"));

// Register services
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IOrderService, OrderService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Add idempotency middleware
app.UseIdempotency();

app.UseAuthorization();
app.MapControllers();

app.Run();