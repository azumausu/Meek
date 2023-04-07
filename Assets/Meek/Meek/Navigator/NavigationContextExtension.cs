using System;
using JetBrains.Annotations;

namespace Meek
{
    public static class NavigationContextExtension
    {
        public static T GetFeatureValue<T>(this NavigationContext self, string key)
        {
            if (!self.Features.TryGetValue(key, out var valueObject)) 
                throw new InvalidOperationException($"{key} does not exist.");
            
            if (valueObject is not T value) 
                throw new InvalidOperationException($"Trying to cast Type {valueObject.GetType().Name} to Type {typeof(T).Name}");

            return value;
        }
        
        [CanBeNull]
        public static T GetFeatureNullableValue<T>(this NavigationContext self, string key)
            where T : class
        {
            if (!self.Features.TryGetValue(key, out var valueObject)) 
                throw new InvalidOperationException($"{key} does not exist.");

            if (valueObject is not T value) return null;
                
            return value;
        } 
    }
}