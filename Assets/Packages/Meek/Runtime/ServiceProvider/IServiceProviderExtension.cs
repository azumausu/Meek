using System;

namespace Meek
{
    public static class IServiceProviderExtension
    {
        public static T GetService<T>(this IServiceProvider self)
        {
            return (T)self.GetService(typeof(T));
        }
    }
}