using Microsoft.Extensions.DependencyInjection;
using SocialNetwork.Application.Interfaces;
using SocialNetwork.Application.Service;

namespace SocialNetwork.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<ICommentService, CommentService>();
            services.AddScoped<IPostService, PostService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IChatService, ChatService>();
            services.AddScoped<IMessageService, MessageService>();
            services.AddScoped<IFriendService, FriendService>();
            services.AddScoped<INotificationService, NotificationService>();


            return services;
        }
    }
}
