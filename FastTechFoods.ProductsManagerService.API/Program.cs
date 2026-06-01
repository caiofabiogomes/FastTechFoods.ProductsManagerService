using FastTechFoods.ProductsManagerService.Application;
using FastTechFoods.ProductsManagerService.Infraestructure;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Sinks.Grafana.Loki;

var builder = WebApplication.CreateBuilder(args);

var serviceName = "FastTechFoods.ProductsServiceManagerAPI";

var configuration = builder.Configuration;

var lokiStringConnection = Environment.GetEnvironmentVariable("CONNECTION_LOKI") ??
                "http://localhost:3100";

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .Enrich.WithSpan()
    .Enrich.WithProperty("Application", serviceName)
    .WriteTo.GrafanaLoki(
        uri: lokiStringConnection,
        labels: new[]
        {
            new LokiLabel { Key = "app", Value = serviceName }
        })
    .CreateLogger();

builder.Host.UseSerilog();

var openTelemetryConnection = Environment.GetEnvironmentVariable("CONNECTION_OPENTELEMETRY") ??
                "http://localhost:4317";

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(serviceName))
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation()
            .AddSource("MassTransit")
            .AddOtlpExporter(options =>
            {
                // 👇 FORÇANDO A URL E O PROTOCOLO 👇
                options.Endpoint = new Uri(openTelemetryConnection);
                options.Protocol = OtlpExportProtocol.Grpc;
            });
    });

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .AddInfrastructure(builder.Configuration)
    .AddApplication();





var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
    db.Database.Migrate();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
