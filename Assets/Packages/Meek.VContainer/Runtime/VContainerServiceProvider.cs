#if MEEK_ENABLE_VCONTAINER
using System;
using System.Threading;
using UnityEngine;
using VContainer;

public class VContainerServiceProvider : IServiceProvider, IDisposable
{
    public readonly IObjectResolver ObjectResolver;
    private bool _isDisposed = false;
        
    public VContainerServiceProvider(IObjectResolver objectResolver)
    {
        ObjectResolver = objectResolver;
    }
    
    public T GetService<T>()
    {
        if (_isDisposed)
            throw new InvalidOperationException("This instance is already disposed.");
        
        return ObjectResolver.Resolve<T>();
    }

    public object GetService(Type type)
    {
        if (_isDisposed)
            throw new InvalidOperationException("This instance is already disposed.");
        
        return ObjectResolver.Resolve(type);
    }
    
    public void Dispose()
    {
        if (_isDisposed) return;
        
        _isDisposed = true;
        ObjectResolver?.Dispose();
    }
}
#endif