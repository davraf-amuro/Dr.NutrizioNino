using Dr.NutrizioNino.Api.Endopints;
using Dr.NutrizioNino.Api.Infrastructure;
using Dr.NutrizioNino.Api.Interfaces;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Api.Services;
using Microsoft.EntityFrameworkCore;

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:5120/");
                      });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//aggiungi i servizi
builder.Services.AddDbContext<DrNutrizioNinoContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DrNutrizioNinoSql"));
});

builder.Services.AddScoped<IFoodsRepository, FoodsRepository>();
builder.Services.AddScoped<FoodsService, FoodsService>();
builder.Services.AddScoped<IBrandsRepository, BrandsReporitoy>();
builder.Services.AddScoped<BrandsService, BrandsService>();

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

app.Run();

