using System;

namespace Meek.NavigationStack
{
    public static class NavigatorAnimationBuilderExtension
    {
        public static IServiceCollection AddNavigatorAnimation(this IServiceCollection self)
        {
            self.AddSingleton<NavigatorAnimationMiddleware>();
            self.AddScope<INavigatorAnimationStrategy, PushNavigatorAnimationStrategy>();
            self.AddScope<INavigatorAnimationStrategy, PopNavigatorAnimationStrategy>();
            self.AddScope<INavigatorAnimationStrategy, RemoveNavigatorAnimationStrategy>();
            self.AddScope<INavigatorAnimationStrategy, InsertNavigatorAnimationStrategy>();
            return self;
        }

        public static INavigatorBuilder UseNavigatorAnimation(this INavigatorBuilder app)
        {
            return app.UseMiddleware<NavigatorAnimationMiddleware>();
        }
    }
}