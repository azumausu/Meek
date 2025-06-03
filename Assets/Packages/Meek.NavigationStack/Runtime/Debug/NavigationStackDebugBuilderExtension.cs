using System;

namespace Meek.NavigationStack.Debugs
{
    public static class NavigationStackDebugBuilderExtension
    {
        public static IServiceCollection AddDebug(this IServiceCollection self, NavigationStackDebugOption option)
        {
            self.AddSingleton(option);
            self.AddSingleton(x => new ServiceRegistrationHandler(x));
            self.AddSingleton<NavigationStackDebugMiddleware>();

            return self;
        }

        public static INavigatorBuilder UseDebug(this INavigatorBuilder self)
        {
            self.ServiceProvider.GetService<ServiceRegistrationHandler>();
            self.UseMiddleware<NavigationStackDebugMiddleware>();

            return self;
        }
    }
}