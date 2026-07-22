using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Infrastructure.Options;
using ArenaPass.Infrastructure.Persistence;
using ArenaPass.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ArenaPass.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<ArenaPassDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ArenaPassDbContext>());

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<PixSettings>(configuration.GetSection(PixSettings.SectionName));

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IQrCodeGenerator, QrCodeGenerator>();
        services.AddScoped<IPixPayloadGenerator, PixPayloadGenerator>();

        return services;
    }
}
