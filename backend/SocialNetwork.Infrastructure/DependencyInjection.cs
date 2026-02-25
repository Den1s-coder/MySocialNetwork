using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SocialNetwork.Domain.Interfaces;
using SocialNetwork.Infrastructure.Repos;
using SocialNetwork.Infrastructure.Security;

namespace SocialNetwork.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<ICommentRepository, CommentRepository>();
            services.AddScoped<IPostRepository, PostRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IChatRepository, ChatRepository>();
            services.AddScoped<IMessageRepository, MessageRepository>();
            services.AddScoped<IFriendshipRepository, FriendshipRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();

            services.Configure<JwtOptions>(configuration.GetSection("JwtOptions"));
            services.AddScoped<IJwtProvider, JwtProvider>();

            return services;
        }
    }
}
