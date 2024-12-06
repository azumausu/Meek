using System;

namespace Meek.NavigationStack
{
    public static class NavigatorAnimationBuilderExtension
    {
        public static IServiceCollection AddNavigatorAnimation(this IServiceCollection self)
        {
            self.AddSingleton<NavigatorAnimationMiddleware>();
            self.AddTransient<PushNavigatorAnimationStrategy>();
            self.AddTransient<PopNavigatorAnimationStrategy>();
            self.AddTransient<RemoveNavigatorAnimationStrategy>();
            self.AddTransient<InsertNavigatorAnimationStrategy>();
            return self;
        }

        public static INavigatorBuilder UseNavigatorAnimation(this INavigatorBuilder app)
        {
            return app.UseMiddleware<NavigatorAnimationMiddleware>();
        }
    }
}