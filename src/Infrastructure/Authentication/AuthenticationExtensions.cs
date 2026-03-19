using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace CleanArchitecture.VerticalSlice.Infrastructure.Authentication;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddForgeRockAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var authOptions = new ForgeRockOptions();
        configuration.GetSection("ForgeRock").Bind(authOptions);
        services.Configure<ForgeRockOptions>(configuration.GetSection("ForgeRock"));

        services.AddMemoryCache();
        services.AddHttpClient<IIntrospectionService, ForgeRockIntrospectionService>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = authOptions.Authority;
                options.Audience = authOptions.Audience;
                options.RequireHttpsMetadata = true;

                // Validate using JWKS first
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = authOptions.Authority,
                    ValidateAudience = true,
                    ValidAudience = authOptions.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        if (context.SecurityToken is System.IdentityModel.Tokens.Jwt.JwtSecurityToken jwtToken)
                        {
                            var introspectionService = context.HttpContext.RequestServices.GetRequiredService<IIntrospectionService>();
                            var isActive = await introspectionService.IsActiveAsync(jwtToken.RawData);

                            if (!isActive)
                            {
                                context.Fail("Token is not active based on introspection.");
                            }
                        }
                    }
                };
            });

        services.AddAuthorization();

        return services;
    }
}
