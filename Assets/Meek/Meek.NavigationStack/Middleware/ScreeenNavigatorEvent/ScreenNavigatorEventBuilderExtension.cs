namespace Meek.NavigationStack
{
    public static class ScreenNavigatorEventBuilderExtension
    {
        public static IServiceCollection AddScreenNavigatorEvent(this IServiceCollection self)
        {
            self.AddSingleton<ScreenNavigatorEventMiddleware>();

            return self;
        }
        
        public static INavigatorBuilder UseScreenNavigatorEvent(this INavigatorBuilder app)
        {
            return app.UseMiddleware<ScreenNavigatorEventMiddleware>();
        }
    }
}