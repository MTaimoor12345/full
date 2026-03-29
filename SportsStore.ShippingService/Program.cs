using MassTransit;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SportsStore.ShippingService.Consumers;
using SportsStore.ShippingService.Data;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration);
    config.WriteTo.Console()
          .WriteTo.File("logs/shipping-service-.log", rollingInterval: RollingInterval.Day)
          .Enrich.WithProperty("ServiceName", "ShippingService")
          .Enrich.FromLogContext();
});

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure SQLite Database
builder.Services.AddDbContext<ShippingDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("ShippingDatabase")
        ?? "Data Source=shipping.db"));

// Configure MassTransit with RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<PaymentApprovedConsumer>();
    
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"] ?? "localhost", "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
            h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
        });
        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging(opts =>
{
    opts.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestPath", httpContext.Request.Path);
        diagnosticContext.Set("RequestMethod", httpContext.Request.Method);
    };
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Ensure database is created and seeded
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ShippingDbContext>();
    db.Database.EnsureCreated();
    SeedData.EnsurePopulated(app);
}

app.Logger.LogInformation("Shipping Service starting. Environment: {Environment}", app.Environment.EnvironmentName);

app.Run();
