using System;

namespace Meek.NavigationStack
{
    public static class InputBuilderExtension
    {
        public static IServiceCollection AddInputLocker(this IServiceCollection self, IInputLocker inputLocker)
        {
            self.AddSingleton(inputLocker);
            self.AddSingleton<InputLockerMiddleware>();

            return self;
        }

        public static INavigatorBuilder UseInputLocker(this INavigatorBuilder app)
        {
            return app.UseMiddleware<InputLockerMiddleware>();
        }
    }
}