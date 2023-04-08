namespace Meek.NavigationStack
{
    public static class ScreenUIBuilderExtension
    {
        public static IServiceCollection AddScreenUI(this IServiceCollection self)
        {
            self.AddTransient<ScreenUI>();
            self.AddSingleton<ScreenUIMiddleware>();
            
            return self;
        }

        public static INavigatorBuilder UseScreenUI(this INavigatorBuilder app)
        {
            return app.UseMiddleware<ScreenUIMiddleware>();
        }
    }
}