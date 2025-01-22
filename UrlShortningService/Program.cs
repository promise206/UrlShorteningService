using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Reflection;
using UrlShortningService.Data;
using UrlShortningService.Services.Implementation;
using Microsoft.Extensions.Caching.Distributed;
using UrlShortningService.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

var config = new ConfigurationBuilder()
    .Build();

Log.Logger = new LoggerConfiguration().Enrich.FromLogContext()
        .CreateLogger();

builder.Services.AddDbContext<UrlShortenerDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

builder.Services.AddMemoryCache();
builder.Services.AddScoped<ICacheService, MemoryCacheService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


// Configure the HTTP request pipeline.
app.UseMiddleware<RateLimitingMiddleware>(100, TimeSpan.FromMinutes(1));
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
