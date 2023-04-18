using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Meek
{
    public static class TypeExtension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AssertImplementation<TService>(this Type implementationType)
        {
            if (implementationType != null && implementationType.GetInterfaces().All(x => x != typeof(TService)))
                throw new ArgumentException($"Type {implementationType} is not implement {typeof(TService)}");
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AssertImplementation(this Type implementationType, Type serviceType)
        {
            if (implementationType != null && implementationType.GetInterfaces().All(x => x != serviceType))
                throw new ArgumentException($"Type {implementationType} is not implement {serviceType}");
        }
    }
}