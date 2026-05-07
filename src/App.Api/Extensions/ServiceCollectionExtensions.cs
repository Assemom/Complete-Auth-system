using System.Text;
using App.DataAccess.Context;
using App.Domain.Entities;
using App.Domain.Enums;
using App.Shared.Constants;
using App.Shared.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using IdentityConstants = App.Shared.Constants.IdentityConstants;

namespace App.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityConfiguration(this IServiceCollection services)
    {
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = ValidationConstants.PasswordMinLength;

            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(IdentityConstants.LockoutMinutes);
            options.Lockout.MaxFailedAccessAttempts = IdentityConstants.MaxFailedAccessAttempts;
            options.Lockout.AllowedForNewUsers = true;

            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = true;
        }).AddEntityFrameworkStores<ApplicationDbContext>()
          .AddDefaultTokenProviders();

        services.AddAuthorization(options =>
        {
            options.AddPolicy(PolicyConstants.RequireAdmin, policy => policy.RequireRole(nameof(Roles.Admin)));
            options.AddPolicy(PolicyConstants.RequireUser, policy => policy.RequireRole(nameof(Roles.User)));
        });

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(nameof(JwtSettings)));

        var jwtSettings = configuration.GetSection(nameof(JwtSettings)).Get<JwtSettings>();
        const int JwtSecretMinimumLength = 32;
        if (jwtSettings is null || string.IsNullOrWhiteSpace(jwtSettings.Secret) || jwtSettings.Secret.Length < JwtSecretMinimumLength)
        {
            throw new InvalidOperationException(ErrorMessages.JwtSecretTooShort);
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret));

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = key,
                ClockSkew = TimeSpan.Zero
            };
        });

        return services;
    }

    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddSwaggerGen();
        services.ConfigureOptions<ConfigureSwaggerOptions>();

        return services;
    }
}
