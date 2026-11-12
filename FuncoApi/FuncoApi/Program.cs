using System.Text.Json;
using FuncoApi.Cache;
using FuncoApi.Cache.objet;
using FuncoApi.Context;
using FuncoApi.Context.Mongo;
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

var builder = WebApplication.CreateBuilder(args);

// Mantiene los nombres JSON de la API en camelCase.
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

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
// Configura los ajustes de correo electrónico desde la configuración.
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
// Usa la misma sección JWT para emitir y validar los tokens.
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

// Registra persistencia, servicios y repositorios que consumen las rutas.
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddScoped<IProveedorDataSource, MongoProveedorDataSource>();
var sqlConnectionString = builder.Configuration.GetConnectionString("FuncoDb")
    ?? throw new InvalidOperationException("La cadena de conexión 'FuncoDb' no está configurada.");
builder.Services.AddDbContext<FuncoDbContext>(options =>
    options.UseSqlServer(sqlConnectionString));
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

// Configura el middleware de la aplicación.
app.UseSwagger();
app.UseSwaggerUI();

// Habilita autenticación y autorización antes de procesar los controladores.
app.UseAuthentication();
app.UseAuthorization();

// Publica los controladores HTTP de la API.
app.MapControllers();

app.Run();