using CohesiveRP.Storage.WebApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

if (builder.Environment.IsDevelopment())
{
    // Formelly known as swagger :'(
    builder.Services.AddOpenApi();
}

// Dependency injection of custom services
CustomServices.AddCustomServices(builder.Services);

var app = builder.Build();
//app.UseMiddleware<StorageWebApiExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Allow unsecure HTTP in Debug to avoid dealing with certificates handling overhead when irrelevant
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();
app.MapControllers();
app.Run();
