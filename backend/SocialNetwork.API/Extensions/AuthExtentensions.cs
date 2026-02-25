using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SocialNetwork.Infrastructure.Security;
using System.Text;

namespace SocialNetwork.API.Extensions
{
    public static class AuthExtentensions
    {
        public static IServiceCollection AddAuth(this IServiceCollection serviceCollection)
        {
            var jwtOptions = serviceCollection.BuildServiceProvider().GetRequiredService<IOptions<JwtOptions>>().Value;

            serviceCollection.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
                    ClockSkew = TimeSpan.FromMinutes(5)
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"].FirstOrDefault();
                        var path = context.HttpContext.Request.Path;

                        if (!string.IsNullOrEmpty(accessToken))
                        {
                            if (path.StartsWithSegments("/chatHub", StringComparison.OrdinalIgnoreCase) ||
                                path.StartsWithSegments("/notificationHub", StringComparison.OrdinalIgnoreCase))
                            {
                                context.Token = accessToken;
                            }
                        }

                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        try
                        {
                            var loggerFactory = context.HttpContext.RequestServices.GetService(typeof(Microsoft.Extensions.Logging.ILoggerFactory)) as Microsoft.Extensions.Logging.ILoggerFactory;
                            var logger = loggerFactory?.CreateLogger("JwtBearer");
                            logger?.LogWarning(context.Exception, "JWT authentication failed during OnMessageReceived for path {Path}", context.Request.Path);
                        }
                        catch { }

                        return Task.CompletedTask;
                    }
                };
            });
            return serviceCollection;
        }
    }
}