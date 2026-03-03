using System.Net;
using System.Net.Sockets;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi;
using CohesiveRP.Core.WebApi.Middlewares;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.Users;

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
//var host = Dns.GetHostEntry(Dns.GetHostName());
//List<string> localIps = new()
//{
//    // TODO: map the accepted origins to configuration + the PORT
//    "http://127.0.0.1:61109",
//    "https://127.0.0.1:61109",
//    "http://localhost:61109",
//    "https://localhost:61109",
//    "http://192.168.0.64:61109",
//    "https://192.168.0.64:61109",
//};

//foreach (var ip in host.AddressList)
//{
//    if (ip.AddressFamily == AddressFamily.InterNetwork)
//    {
//        localIps.Add($"http://{ip}:61109");
//        localIps.Add($"https://{ip}:61109");
//    }
//}

//options.AddPolicy(CORS_WEB_CLIENT_POLICY_NAME, policy =>
//{
//    policy.WithOrigins(localIps.ToArray())
//          .AllowAnyHeader()
//          .AllowAnyMethod()
//          .AllowCredentials();
//});

//options.AddPolicy("DevCors", policy =>
//{
//    // Accept mismatch between localhost and 127.0.0.1 since they're equivalent and should be considered the SAME
//    policy.WithOrigins(
//        "http://localhost:61109",
//        "http://127.0.0.1:61109",
//        "http://192.168.0.64:61109"
//    )
//    .AllowAnyHeader()
//    .AllowAnyMethod();
//});
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        builder =>
        {
            builder.AllowAnyOrigin();
            builder.AllowAnyHeader();
            builder.AllowAnyMethod();
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

//app.UseRouting();

// Start with CORS policies handling
app.UseCors();

// Allow unsecure HTTP in Debug to avoid dealing with certificates handling overhead when irrelevant
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
