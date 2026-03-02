using Asp.Versioning;
using Dr.NutrizioNino.Api.dto;
using Dr.NutrizioNino.Api.Endopints;
using Dr.NutrizioNino.Api.Infrastructure;
using Dr.NutrizioNino.Api.Middleware;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Api.Services;
using Dr.NutrizioNino.Api.Transformers;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;
using TinyHelpers.AspNetCore.Extensions;
using TinyHelpers.AspNetCore.OpenApi;

try
{
    var permitGetPost = "permitGetPost";
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services);
    });
    Log.Information("Application building..................");

    //carico i parametri di configurazione dal config
    builder.Configuration.AddEnvironmentVariables(); //si assicura di ereditare le variabili d'ambiente, serve soprattutto in docker
    builder.Services.Configure<AuthorizationTokensDto>(builder.Configuration.GetSection("AuthorizationTokens"));

    //abilito il versionamento delle api
    builder.Services.AddApiVersioning(options =>
    {
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
        options.DefaultApiVersion = ApiVersionFactory.Version1;
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
    }).AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

    //uso OpenApi di microsoft    
    builder.Services.AddOpenApi(options =>
    {
        //aggiungo informazioni sul documento
        options.AddDocumentTransformer<AddDocumentInformations>();
        //avviso Openapi che deve tener conto degli headers di sicurezza (per la UI)
        options.AddOperationTransformer<AddHeaders>();
        options.AddOperationTransformer<AddGenericsInformations>();
        options.AddDefaultProblemDetailsResponse();
    });

    builder.Services.AddDefaultProblemDetails();
    builder.Services.AddDefaultExceptionHandler();


    //aggiungi i servizi
    builder.Services.AddDbContext<DrNutrizioNinoContext>(options =>
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("DrNutrizioNinoSql"));
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    });

    builder.Services.AddScoped<DrRepository>();
    builder.Services.AddScoped<DrService>();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy(name: permitGetPost
            , policy =>
            {
                policy.AllowAnyOrigin();
                policy.AllowAnyHeader();
                policy.WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS");
            });
    });

    var app = builder.Build();
    Log.Information("Application builded..................");

    // Configure the HTTP request pipeline.
    //creo il set versionamento per gli endpoint
    var versionSet = app.NewApiVersionSet()
        .HasApiVersion(ApiVersionFactory.Version1)
        .ReportApiVersions()
        .Build();

    // Configure the HTTP request pipeline.
    //if (app.Environment.IsDevelopment())
    //{
    app.MapOpenApi();
    //attivo le UI
    app.MapScalarApiReference(options =>
    {
        options.DarkMode = false;
        options.DefaultHttpClient = new KeyValuePair<ScalarTarget, ScalarClient>(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });

    app.UseCors(permitGetPost);
    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseExceptionHandler();
    app.UseStatusCodePages();

    //aggiungi gli endpoint 
    app.MapsFoodsEndpoints(versionSet);
    app.MapsBrandsEndpoints(versionSet);
    app.MapsNutrientsEndpoints(versionSet);
    app.MapUnitsOfMeasureEndpoints(versionSet);

    //carica i middleware
    app.UseMiddleware<HttpContextLogger>();
    app.UseMiddleware<ValidatorMiddleware>();

    //Log.Information($"Security Protocols Allowed: {ServicePointManager.SecurityProtocol}");
    Log.Information("Application running..................");
    app.Run();

}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
    throw;
}

//non mi ricordo ma mi sembra importante
public partial class Program { }
