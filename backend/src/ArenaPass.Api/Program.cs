using System.Text;
using System.Text.Json.Serialization;
using ArenaPass.Api.Middleware;
using ArenaPass.Application;
using ArenaPass.Infrastructure;
using ArenaPass.Infrastructure.Options;
using ArenaPass.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

const string CorsPolicyName = "ArenaPassFrontend";

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? new JwtSettings();

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                string.IsNullOrEmpty(jwtSettings.Secret) ? new string('x', 32) : jwtSettings.Secret))
        };
    });

builder.Services.AddAuthorization();

// Proteção do endpoint público de validação de convite contra tentativa e erro
// de tokens: janela fixa por IP. O limite é folgado pro uso real (portaria
// escaneando QRs) e apertado o bastante pra inviabilizar enumeração.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("convite-validar", httpContext =>
        System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
            {
                PermitLimit = 20,
                Window = TimeSpan.FromMinutes(1)
            }));
});

var frontendOrigins = builder.Configuration.GetSection("Cors:FrontendOrigins").Get<string[]>()
                      ?? ["http://localhost:4200"];

// Domínio base dos tenants: qualquer subdomínio https dele é aceito automaticamente,
// pra não precisar cadastrar cada espaço novo (arena10, hrtennis, ...) na env var do Render.
var corsBaseDomain = builder.Configuration["Cors:BaseDomain"] ?? "wnlabs.com.br";

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        policy.SetIsOriginAllowed(origin =>
                frontendOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase) ||
                (Uri.TryCreate(origin, UriKind.Absolute, out var uri) &&
                 uri.Scheme == Uri.UriSchemeHttps &&
                 (uri.Host.Equals(corsBaseDomain, StringComparison.OrdinalIgnoreCase) ||
                  uri.Host.EndsWith($".{corsBaseDomain}", StringComparison.OrdinalIgnoreCase))))
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "ArenaPass API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Informe o token JWT (sem o prefixo 'Bearer')."
    });
    options.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
    {
        { new OpenApiSecuritySchemeReference("Bearer"), new List<string>() }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrWhiteSpace(connectionString))
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ArenaPassDbContext>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<ArenaPass.Application.Common.Interfaces.IPasswordHasher>();

    // Fora de Development a senha inicial do Master tem que vir do ambiente —
    // sem fallback embutido, pra nunca subir produção com credencial conhecida.
    var masterPassword = builder.Configuration["Seed:MasterPassword"];
    if (string.IsNullOrWhiteSpace(masterPassword))
    {
        if (!app.Environment.IsDevelopment())
        {
            throw new InvalidOperationException(
                "Seed:MasterPassword não configurada — defina a variável de ambiente Seed__MasterPassword para inicializar fora de Development.");
        }

        masterPassword = "Master@123";
    }

    await context.Database.MigrateAsync();
    await ArenaPassDbContextSeed.SeedAsync(context, passwordHasher,
        seedEspacoDemo: app.Environment.IsDevelopment(), masterPassword);
}

app.UseArenaPassExceptionHandling();

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors(CorsPolicyName);

app.UseRateLimiter();

app.UseAuthentication();
app.UseArenaPassTenantResolution();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok("ArenaPass API"));
app.MapControllers();

app.Run();

public partial class Program;
