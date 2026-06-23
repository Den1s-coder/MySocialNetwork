using SocialNetwork.Application.Events;
using SocialNetwork.Application.Interfaces;
using SocialNetwork.Application.Service;

namespace SocialNetwork.API.Extensions
{
    public static class MediatorExtensions
    {
        public static IServiceCollection AddMediator(this IServiceCollection services)
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(PostCreatedEvent).Assembly));
            services.AddScoped<IEventDispatcher, EventDispatcher>();
            return services;
        }
    }
}
