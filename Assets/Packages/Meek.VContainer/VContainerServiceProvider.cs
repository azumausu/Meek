using System;
using VContainer;
using IServiceProvider = Meek.IServiceProvider;

public class VContainerServiceProvider : IServiceProvider
{
    public readonly IObjectResolver ObjectResolver;
        
    public VContainerServiceProvider(IObjectResolver objectResolver)
    {
        ObjectResolver = objectResolver;
    }

    public T GetService<T>()
    {
        return ObjectResolver.Resolve<T>();
    }

    public object GetService(Type type)
    {
        return ObjectResolver.Resolve(type);
    }
}