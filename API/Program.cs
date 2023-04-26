using API.Extensions;

const string ClientAppOrigin = "http://localhost:4200";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services
    .ApplicationServices(builder.Configuration)
    .AddIdentityServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseCors(builder => builder
    .AllowAnyHeader()
    .AllowAnyMethod()
    .WithOrigins(ClientAppOrigin));

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
