using System;

namespace Meek
{
    public static class INavigatorBuilderExtension
    {
        public static INavigatorBuilder UseMiddleware<T>(this INavigatorBuilder self) where T : IMiddleware
        {
            return self.Use(next =>
            {
                return async context =>
                {
                    var middleware = self.ServiceProvider.GetService<T>();
                    await middleware.InvokeAsync(context, next);
                };
            });
        }
    }
}