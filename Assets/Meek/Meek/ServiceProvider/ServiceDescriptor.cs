using System;
using JetBrains.Annotations;

namespace Meek
{
    public class ServiceDescriptor
    {
        public ServiceLifeTime LifeTime { get; init; }
        
        public Type ServiceType { get; init; }
        
        public Type ImplementationType { get; init; }
        
        [CanBeNull] public object ImplementationInstance { get; init; }
        
        [CanBeNull]
        public Func<IServiceProvider, object> ImplementationFactory { get; init; }


        public bool IsSingleton()
        {
            return LifeTime == ServiceLifeTime.Singleton;
        }

        public bool IsScoped()
        {
            return LifeTime == ServiceLifeTime.Scoped; 
        }

        public bool IsTransient()
        {
            return LifeTime == ServiceLifeTime.Transient;
        }

        public bool IsImplementationType()
        {
            return ServiceType != ImplementationType;
        }

        public bool IsInstance()
        {
            return ImplementationInstance != null;
        }

        public bool IsFactory()
        {
            return ImplementationFactory != null;
        }
    }
}