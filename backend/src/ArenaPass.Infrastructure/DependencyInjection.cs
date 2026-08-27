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

        services.AddScoped<ICurrentTenant, CurrentTenant>();

        services.AddDbContext<ArenaPassDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ArenaPassDbContext>());

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<PixSettings>(configuration.GetSection(PixSettings.SectionName));
        services.Configure<BrevoSettings>(configuration.GetSection(BrevoSettings.SectionName));
        services.Configure<NotificacoesSettings>(configuration.GetSection(NotificacoesSettings.SectionName));
        services.AddScoped<INotificacoesConfiguracao, NotificacoesConfiguracao>();

        services.Configure<LgpdSettings>(configuration.GetSection(LgpdSettings.SectionName));
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            services.AddHostedService<ExpurgoCpfConvitesService>();
        }

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IQrCodeGenerator, QrCodeGenerator>();
        services.AddScoped<IPixPayloadGenerator, PixPayloadGenerator>();
        services.AddHttpClient<IEmailSender, BrevoEmailSender>();

        return services;
    }
}
