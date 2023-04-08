using System;
using System.Collections.Generic;
using System.Linq;

namespace Sample
{
    /// <summary>
    /// Service-Locatorパターン
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _dictionary = new Dictionary<Type, object>();

        public static bool TryRegister<TInstance>(TInstance instance)
        {
            return TryRegister<TInstance, TInstance>(instance);
        }
        
        public static bool TryRegister<TInterface, TInstance>(TInstance instance)
        {
            var type = typeof(TInterface);
            if (!_dictionary.ContainsKey(type))
            {
                _dictionary.Add(type, instance);
                return true;
            }

            return false;
        }
        
        public static bool TryUnregister<TInstance>(TInstance instance)
        {
            return TryUnregister<TInstance, TInstance>(instance);
        }

        public static bool TryUnregister<TInterface, TInstance>(TInstance instance)
        {
            var type = typeof(TInterface);
            if (!_dictionary.ContainsKey(type)) return false;
            
            _dictionary.Remove(type);
            return true;

        }

        public static T Resolve<T>() where T : class
        {
            var type = typeof(T);
            if (_dictionary.TryGetValue(type, out object value)) return value as T;
            
            return null;
        }

        public static void Clear()
        {
            foreach (var service in _dictionary.Select(x => x.Value)
                .Select(x => x as IDisposable)
                .Where(x => x != null))
            {
                service.Dispose();
            }
            
            _dictionary.Clear();
        }
    }
}