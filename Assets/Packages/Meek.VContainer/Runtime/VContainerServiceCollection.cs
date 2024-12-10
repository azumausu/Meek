using System;
using System.Linq;
using Meek;
using VContainer;
using VContainer.Unity;
using IContainerBuilder = Meek.IContainerBuilder;
using Object = UnityEngine.Object;

public class VContainerServiceCollection : IContainerBuilder
{
    private IObjectResolver _parentObjectResolver;
    private ServiceCollection _serviceCollection = new();

    public IServiceCollection ServiceCollection => _serviceCollection;

    public VContainerServiceCollection(IServiceProvider parentServiceProvider = null)
    {
        if (parentServiceProvider == null) return;

        var vContainerServiceProvider = parentServiceProvider as VContainerServiceProvider;
        if (vContainerServiceProvider == null)
        {
            throw new ArgumentException("parentServiceProvider is not VContainerServiceProvider");
        }

        _parentObjectResolver = vContainerServiceProvider.ObjectResolver;
    }

    public IServiceProvider Build()
    {
        _serviceCollection.MakeReadOnly();
        Action<global::VContainer.IContainerBuilder> installer = containerBuilder =>
        {
            var instanceGroupList = _serviceCollection
                .Where(y => y.IsInstance())
                .GroupBy(y => y.ImplementationInstance)
                .ToList();
            foreach (var d in _serviceCollection)
            {
                // Factory
                if (d.IsFactory())
                {
                    var lifetime = d.LifeTime switch
                    {
                        ServiceLifeTime.Singleton => Lifetime.Singleton,
                        ServiceLifeTime.Scoped => Lifetime.Scoped,
                        ServiceLifeTime.Transient => Lifetime.Transient,
                        _ => throw new ArgumentOutOfRangeException()
                    };

                    var registrationBuilder = new FuncRegistrationBuilder(x =>
                    {
                        var serviceProvider = new VContainerServiceProvider(x);
                        return d.ImplementationFactory!(serviceProvider);
                    }, d.ServiceType, d.ImplementationType, lifetime);
                    containerBuilder.Register(registrationBuilder);
                    continue;
                }

                // Instance
                if (d.IsInstance())
                {
                    var instanceGroup = instanceGroupList.FirstOrDefault(group => group.Key == d.ImplementationInstance);
                    if (instanceGroup != null)
                    {
                        var instanceRegisterBuilder = containerBuilder.RegisterInstance(instanceGroup.Key);
                        foreach (var descriptor in instanceGroup) instanceRegisterBuilder.As(descriptor.ServiceType);
                        instanceGroupList.Remove(instanceGroup);
                        continue;
                    }
                }

                // Implementation
                {
                    var lifetime = d.LifeTime switch
                    {
                        ServiceLifeTime.Singleton => Lifetime.Singleton,
                        ServiceLifeTime.Scoped => Lifetime.Scoped,
                        ServiceLifeTime.Transient => Lifetime.Transient,
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    var registrationBuilder = d.IsImplementationType()
                        ? containerBuilder.Register(d.ImplementationType, lifetime).As(d.ServiceType)
                        : containerBuilder.Register(d.ServiceType, lifetime);
                }
            }
        };

        if (_parentObjectResolver == null)
        {
            var lifetimeScope = LifetimeScope.Create(installer);
            Object.DontDestroyOnLoad(lifetimeScope);
            return new VContainerServiceProvider(lifetimeScope.Container);
        }

        var scopedObjectResolver = _parentObjectResolver.CreateScope(installer);
        return new VContainerServiceProvider(scopedObjectResolver);
    }
}