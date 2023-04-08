namespace Meek.NavigationStack
{
    public static class ScreenLifecycleEventBuilderExtension
    {
        public static IServiceCollection AddScreenLifecycleEvent(this IServiceCollection self)
        {
            self.AddSingleton<ScreenLifecycleEventMiddleware>();

            return self;
        }
        
        public static INavigatorBuilder UseScreenLifecycleEvent(this INavigatorBuilder app)
        {
            return app.UseMiddleware<ScreenLifecycleEventMiddleware>();
        }
    }
}