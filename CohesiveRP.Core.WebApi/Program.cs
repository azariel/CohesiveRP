using System.Net;
using System.Net.Sockets;
using CohesiveRP.Core.WebApi;
using CohesiveRP.Core.WebApi.Middlewares;

const string CORS_WEB_CLIENT_POLICY_NAME = "AllowWebClientPolicy";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

if (builder.Environment.IsDevelopment())
{
    // Formelly known as swagger :'(
    builder.Services.AddOpenApi();
}

// Add CORS services
// Build the ipv4 local addresses so we can map them to default CORS
var host = Dns.GetHostEntry(Dns.GetHostName());
List<string> localIps = new()
{
    // TODO: map the accepted origins to configuration + the PORT
    "http://127.0.0.1:61109",
    "https://127.0.0.1:61109",
    "http://localhost:61109",
    "https://localhost:61109",
};

foreach (var ip in host.AddressList)
{
    if (ip.AddressFamily == AddressFamily.InterNetwork)
    {
        localIps.Add($"http://{ip}:61109");
        localIps.Add($"https://{ip}:61109");
    }
}

builder.Services.AddCors(options =>
{
    options.AddPolicy(CORS_WEB_CLIENT_POLICY_NAME, policy =>
    {
        policy.WithOrigins(localIps.ToArray())
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Dependency injection of custom services
CustomServices.AddCustomServices(builder.Services);

var app = builder.Build();
app.UseMiddleware<CoreWebApiExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Start with CORS policies handling
app.UseCors(CORS_WEB_CLIENT_POLICY_NAME);

// Allow unsecure HTTP in Debug to avoid dealing with certificates handling overhead when irrelevant
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
