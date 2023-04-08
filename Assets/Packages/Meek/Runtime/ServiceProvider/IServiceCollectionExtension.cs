using System;

namespace Meek
{
    public static class IServiceCollectionExtension
    {
        public static IServiceCollection AddSingleton<TImplementationInstance>(this IServiceCollection self, TImplementationInstance instance)
        {
            var instanceType = typeof(TImplementationInstance);
            return self.AddSingleton(instanceType, instanceType, instance);
        }

        public static IServiceCollection AddSingleton<TService>(this IServiceCollection self)
        {
            return self.AddSingleton(typeof(TService));
        }
        
        public static IServiceCollection AddSingleton<TService, TImplementationInstance>(this IServiceCollection self, TImplementationInstance instance)
            where TImplementationInstance : TService
        {
            return self.AddSingleton(typeof(TService), typeof(TImplementationInstance), instance);
        }

        public static IServiceCollection AddSingleton<TService, TImplementation>(this IServiceCollection self)
            where TImplementation : TService
        {
            return self.AddSingleton(typeof(TService), typeof(TImplementation));
        }

        public static IServiceCollection AddSingleton(this IServiceCollection self, Type serviceType)
        {
            return self.AddSingleton(serviceType, serviceType);
        }
        
        public static IServiceCollection AddSingleton<TService>(
            this IServiceCollection self,
            Func<IServiceProvider, TService> implementationFactory
        ) where TService : class
        {
            return self.AddSingleton(typeof(TService), implementationFactory:implementationFactory);
        }

        public static IServiceCollection AddSingleton<TService, TImplementation>(
            this IServiceCollection self,
            Func<IServiceProvider, TImplementation> implementationFactory
        ) 
            where TService : class 
            where TImplementation : class, TService
        {
            return self.AddSingleton(typeof(TService), typeof(TImplementation), implementationFactory:implementationFactory);
        }
        
        public static IServiceCollection AddSingleton(
            this IServiceCollection self,
            Type serviceType,
            Func<IServiceProvider, object> implementationFactory)
        {
            return self.AddSingleton(serviceType, serviceType, implementationFactory:implementationFactory);
        }
        
        public static IServiceCollection AddSingleton(this IServiceCollection self, Type serviceType, Type implementationType, object instance = null, Func<IServiceProvider, object> implementationFactory = null)
        {
            self.Add(CreateDescriptor(ServiceLifeTime.Singleton, serviceType, implementationType, instance, implementationFactory));

            return self;
        }
        
        public static IServiceCollection AddScope<TService>(this IServiceCollection self)
        {
            return self.AddScope(typeof(TService));
        }

        public static IServiceCollection AddScope<TService, TImplementation>(this IServiceCollection self)
            where TImplementation : TService
        {
            return self.AddScope(typeof(TService), typeof(TImplementation));
        }

        public static IServiceCollection AddScope(this IServiceCollection self, Type serviceType)
        {
            return self.AddScope(serviceType, serviceType);
        }
        
        public static IServiceCollection AddScope<TService>(
            this IServiceCollection self,
            Func<IServiceProvider, TService> implementationFactory
        ) where TService : class
        {
            return self.AddScope(typeof(TService), implementationFactory);
        }

        public static IServiceCollection AddScope<TService, TImplementation>(
            this IServiceCollection self,
            Func<IServiceProvider, TImplementation> implementationFactory
        ) 
            where TService : class 
            where TImplementation : class, TService
        {
            return self.AddScope(typeof(TService), typeof(TImplementation), implementationFactory);
        }
        
        public static IServiceCollection AddScope(
            this IServiceCollection self,
            Type serviceType,
            Func<IServiceProvider, object> implementationFactory)
        {
            return self.AddScope(serviceType, serviceType, implementationFactory);
        }
        
        public static IServiceCollection AddScope(
            this IServiceCollection self,
            Type serviceType,
            Type implementationType,
            Func<IServiceProvider, object> implementationFactory = null
            )
        {
            self.Add(CreateDescriptor(ServiceLifeTime.Scoped, serviceType, implementationType, implementationFactory:implementationFactory));

            return self;
        }

        public static IServiceCollection AddTransient<TService>(this IServiceCollection self)
        {
            return self.AddTransient(typeof(TService));
        }

        public static IServiceCollection AddTransient<TService, TImplementation>(this IServiceCollection self)
            where TImplementation : TService
        {
            return self.AddTransient(typeof(TService), typeof(TImplementation));
        }

        public static IServiceCollection AddTransient(this IServiceCollection self, Type serviceType)
        {
            return self.AddTransient(serviceType, serviceType);
        }
        
        public static IServiceCollection AddTransient<TService>(
            this IServiceCollection self,
            Func<IServiceProvider, TService> implementationFactory
        ) where TService : class
        {
            return self.AddTransient(typeof(TService), implementationFactory);
        }

        public static IServiceCollection AddTransient<TService, TImplementation>(
            this IServiceCollection self,
            Func<IServiceProvider, TImplementation> implementationFactory
        ) 
            where TService : class 
            where TImplementation : class, TService
        {
            return self.AddTransient(typeof(TService), typeof(TImplementation), implementationFactory);
        }
        
        public static IServiceCollection AddTransient(
            this IServiceCollection self,
            Type serviceType,
            Func<IServiceProvider, object> implementationFactory)
        {
            return self.AddTransient(serviceType, serviceType, implementationFactory);
        }
        
        public static IServiceCollection AddTransient(this IServiceCollection self, Type serviceType, Type implementationType, Func<IServiceProvider, object> implementationFactory = null)
        {
            self.Add(CreateDescriptor(ServiceLifeTime.Transient, serviceType, implementationType, implementationFactory:implementationFactory));

            return self;
        }

        private static ServiceDescriptor CreateDescriptor(
            ServiceLifeTime serviceTime,
            Type serviceType,
            Type implementationType,
            object implementationInstance = null,
            Func<IServiceProvider, object> implementationFactory = null
            )
        {
            return new ServiceDescriptor()
            {
                LifeTime = serviceTime,
                ServiceType = serviceType,
                ImplementationType = implementationType,
                ImplementationInstance = implementationInstance,
                ImplementationFactory = implementationFactory,
            }; 
        }
    }
}