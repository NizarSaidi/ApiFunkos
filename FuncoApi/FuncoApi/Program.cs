using System.Reflection;
using System.Text.Json;
using FuncoApi.Cache;
using FuncoApi.Cache.objet;
using FuncoApi.Context;
using FuncoApi.Context.Mongo;
using FuncoApi.Data;
using FuncoApi.Repositories;
using FuncoApi.Repositories.proveedoresRepository;
using FuncoApi.Repositories.userRepository;
using FuncoApi.Services.CategoriaService;
using FuncoApi.Services.FunkosService;
using FuncoApi.Services.ProveedoresService;
using Microsoft.EntityFrameworkCore;
using FuncoApi.Email;
using FuncoApi.Email.service;
using FuncoApi.Services.Authorification;
using FuncoApi.Tokens;
using FunkoApi.Routes;
using Microsoft.OpenApi;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Mantiene los nombres JSON de la API en camelCase.
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});
// Configura la caché de salida para mejorar el rendimiento de las respuestas.
builder.Services.AddOutputCache();
// Configura Swagger con autenticación JWT en la documentación.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Token JWT"
        });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document, null)] = []
        });
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1.0.0",
        Title = "ProductosApp API",
        Description = "API REST para gestion de Productos",
        Contact = new OpenApiContact
        {
            Name = "Jose Luis Gonzalez",
            Email = "jose@example.com"
        }
    });

    // Incluir comentarios XML (solo si existe: si no, IncludeXmlComments lanza FileNotFoundException)
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    options.CustomSchemaIds(type => type.FullName); // Evita conflictos de nombres de tipos en Swagger
});

// Selecciona la implementación de caché según el entorno.
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddScoped<
        ICacheService,
        RedisCacheService>();
}
else
{
    builder.Services.AddMemoryCache();
    builder.Services.AddScoped<
        ICacheService,
        MemoryCacheService>();
}

// Configura CORS para permitir solicitudes desde el frontend.
builder.Services.AddCors(options =>
{
    options.AddPolicy("Production", policy =>
    {
        policy.WithOrigins(
                "https://mi-frontend.com",
                "https://admin.mi-frontend.com"
            )
            .WithMethods("GET", "POST", "PUT", "DELETE")
            .WithHeaders("Content-Type", "Authorization")
            .AllowCredentials();
    });
});
// Configura los ajustes de correo electrónico desde la configuración.
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
// Usa la misma sección JWT para emitir y validar los tokens.
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

// Selecciona persistencia en memoria o servidores reales mediante Database:UseInMemory.
var useInMemoryDatabases = builder.Configuration.GetValue(
    "Database:UseInMemory",
    builder.Environment.IsDevelopment());
if (useInMemoryDatabases)
{
    builder.Services.AddDbContext<FuncoDbContext>(options =>
        options.UseInMemoryDatabase("FuncoApi-Development"));
    builder.Services.AddSingleton<IProveedorDataSource, InMemoryProveedorDataSource>();
}
else
{
    var sqlConnectionString = builder.Configuration.GetConnectionString("FuncoDb")
        ?? throw new InvalidOperationException("La cadena de conexión 'FuncoDb' no está configurada.");
    builder.Services.AddDbContext<FuncoDbContext>(options =>
        options.UseSqlServer(sqlConnectionString));
    builder.Services.AddSingleton<MongoDbContext>();
    builder.Services.AddScoped<IProveedorDataSource, MongoProveedorDataSource>();
}

builder.Services.AddScoped<IFunkoRepository, FunkoRepository>();
builder.Services.AddScoped<IUserRepository, UsuarioRepository>();
builder.Services.AddScoped<IFunkosService, FunkoService>();
builder.Services.AddScoped<IProveedorRepository, ProveedorRepository>();
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IProveedorService, ProveedorService>();
builder.Services.AddScoped<IJwtTokens, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IEmailTransport, MailKitEmailTransport>();
builder.Services.AddControllers();

// Configura la autenticación y autorización con JWT.
var jwtBuilder = new JWTBuilder(builder);
jwtBuilder.ConfigureJWT();

var app = builder.Build();
// Crea el esquema SQL si todavía no existe.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<FuncoDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}
if (app.Environment.IsDevelopment())
{
    await DevelopmentDataSeeder.SeedAsync(
        app.Services,
        app.Configuration,
        app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("DevelopmentDataSeeder"));
}
// Configura la caché de salida para mejorar el rendimiento de las respuestas.
app.UseOutputCache();

// Configura el middleware de la aplicación.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configura CORS para permitir solicitudes desde el frontend.
app.UseCors("Production");
// Habilita autenticación y autorización antes de procesar los controladores.
app.UseAuthentication();
app.UseAuthorization();

// Publica los controladores HTTP de la API.
app.MapControllers();

app.Run();