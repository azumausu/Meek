using System;

namespace Meek.NavigationStack
{
    public static class InputBuilderExtension
    {
        public static IServiceCollection AddInputLocker(this IServiceCollection self, Action<InputLockerOption> configure)
        {
            var option = new InputLockerOption();
            configure.Invoke(option);

            self.AddSingleton(option);
            self.AddSingleton(option.InputLocker);
            self.AddSingleton<InputLockerMiddleware>();
            
            return self;
        }
        
        public static INavigatorBuilder UseInputLocker(this INavigatorBuilder app)
        {
            return app.UseMiddleware<InputLockerMiddleware>();
        }
    }
}