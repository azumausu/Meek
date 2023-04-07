using System;

namespace Meek.NavigationStack
{
    public static class GlobalInputBuilderExtension
    {
        public static IServiceCollection AddGlobalInputLocker(this IServiceCollection self, Action<GlobalInputLockerOption> configure)
        {
            var option = new GlobalInputLockerOption();
            configure.Invoke(option);

            self.AddSingleton(option);
            self.AddSingleton(option.GlobalInputLocker);
            self.AddSingleton<GlobalInputLockerMiddleware>();
            
            return self;
        }
        
        public static INavigatorBuilder UseGlobalInputLocker(this INavigatorBuilder app)
        {
            return app.UseMiddleware<GlobalInputLockerMiddleware>();
        }
    }
}