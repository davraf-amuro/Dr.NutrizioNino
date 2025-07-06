using Dr.NutrizioNino.Api.Endopints;
using Dr.NutrizioNino.Api.Infrastructure;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Api.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
);

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:5120", "http://localhost:5173");
                      });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//aggiungi i servizi
builder.Services.AddDbContext<DrNutrizioNinoContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DrNutrizioNinoSql"));
    options.EnableSensitiveDataLogging();
    options.EnableDetailedErrors();
});

builder.Services.AddScoped<DrRepository>();
builder.Services.AddScoped<DrService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(MyAllowSpecificOrigins);
app.UseHttpsRedirection();
app.UseStaticFiles();

//aggiungi gli endpoint 
app.MapsFoodsEndpoints();
app.MapsBrandsEndpoints();
app.MapsNutrientsEndpoints();
app.MapUnitsOfMeasureEndpoints();

app.Run();
public partial class Program() { }

