using System.Text;
using Asp.Versioning;
using Dr.NutrizioNino.Api.Endpoints;
using Dr.NutrizioNino.Api.Infrastructure;
using Dr.NutrizioNino.Api.Middleware;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Api.Services;
using Dr.NutrizioNino.Api.Transformers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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
    builder.Configuration.AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);
    builder.Configuration.AddEnvironmentVariables(); //si assicura di ereditare le variabili d'ambiente, serve soprattutto in docker
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
    builder.Services.AddExceptionHandler<DatabaseExceptionHandler>();
    builder.Services.AddDefaultExceptionHandler();

    //aggiungi i servizi
    builder.Services.AddDbContext<DrNutrizioNinoContext>(options =>
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("DrNutrizioNinoSql"));
        if (builder.Environment.IsDevelopment())
        {
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
        }
    });

    // Identity
    builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(opt =>
    {
        opt.Password.RequiredLength = 8;
        opt.Password.RequireNonAlphanumeric = false;
        opt.Password.RequireUppercase = false;
        opt.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<DrNutrizioNinoContext>()
    .AddDefaultTokenProviders();

    // JWT Bearer — sovrascrive esplicitamente gli scheme default impostati da AddIdentity
    var jwtSecret = builder.Configuration["Jwt:Secret"]!;
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.FromMinutes(5)
        };
    });

    // Authorization
    builder.Services.AddAuthorization(opt =>
    {
        opt.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
    });

    builder.Services.AddScoped<DrRepository>();
    builder.Services.AddScoped<BrandService>();
    builder.Services.AddScoped<FoodService>();
    builder.Services.AddScoped<DishService>();
    builder.Services.AddScoped<NutrientService>();
    builder.Services.AddScoped<UnitsOfMeasureService>();
    builder.Services.AddScoped<SupermarketService>();
    builder.Services.AddScoped<CategoryService>();
    builder.Services.AddScoped<AuthService>();
    builder.Services.AddScoped<AdminUserService>();
    builder.Services.AddScoped<UserProfileService>();
    builder.Services.AddScoped<DailySimulationService>();

    var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [];
    builder.Services.AddCors(options =>
    {
        options.AddPolicy(name: permitGetPost
            , policy =>
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyHeader()
                      .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
                      .AllowCredentials();
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
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options.DarkMode = false;
            options.DefaultHttpClient = new KeyValuePair<ScalarTarget, ScalarClient>(ScalarTarget.CSharp, ScalarClient.HttpClient);
        });
    }

    app.UseCors(permitGetPost);
    app.UseHttpsRedirection();
    app.UseExceptionHandler();
    app.UseStatusCodePages();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseMiddleware<HttpContextLogger>();

    //aggiungi gli endpoint
    app.MapsFoodsEndpoints(versionSet);
    app.MapsDishesEndpoints(versionSet);
    app.MapsBrandsEndpoints(versionSet);
    app.MapsNutrientsEndpoints(versionSet);
    app.MapUnitsOfMeasureEndpoints(versionSet);
    app.MapsSupermarketsEndpoints(versionSet);
    app.MapsCategoriesEndpoints(versionSet);
    app.MapsAuthEndpoints(versionSet, app.Environment);
    app.MapsAdminEndpoints(versionSet);
    app.MapsUserProfileEndpoints(versionSet);
    app.MapDailySimulationEndpoints(versionSet);

    // SEED: garantisce che i ruoli esistano al primo avvio
    using (var seedScope = app.Services.CreateScope())
    {
        var adminService = seedScope.ServiceProvider.GetRequiredService<AdminUserService>();
        await adminService.EnsureRolesExistAsync();
    }

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
