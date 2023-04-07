using System;

namespace Meek.NavigationStack
{
    public static class NavigatorAnimationBuilderExtension
    {
        public static IServiceCollection AddNavigatorAnimation(this IServiceCollection self, Action<NavigatorAnimationOption> configure)
        {
            var option = new NavigatorAnimationOption();

            configure(option);

            foreach (var moduleType in option.Strategies) self.AddSingleton(moduleType);
            self.AddSingleton(option);
            self.AddSingleton<NavigatorAnimationMiddleware>();
            return self;
        }
        
        public static INavigatorBuilder UseNavigatorAnimation(this INavigatorBuilder app)
        {
            return app.UseMiddleware<NavigatorAnimationMiddleware>();
        }
    }
}