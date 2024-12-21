#if MEEK_ENABLE_VCONTAINER
using System;
using System.Collections.Generic;
using VContainer;

public class FuncRegistrationBuilder : RegistrationBuilder
{
    private readonly Func<IObjectResolver, object> implementationProvider;
    private readonly Type _serviceType;
    private readonly Type _implementationType;
    private readonly Lifetime _lifetime;
        
    public FuncRegistrationBuilder(
        Func<IObjectResolver, object> implementationProvider,
        Type serviceType,
        Type implementationType,
        Lifetime lifetime) : base(implementationType, lifetime)
    {
        this.implementationProvider = implementationProvider;
        _serviceType = serviceType;
        _implementationType = implementationType;
        _lifetime = lifetime;
        this.As(serviceType);
    }

    public override Registration Build()
    {
        var spawner = new FuncInstanceProvider(implementationProvider);
        return new Registration(_implementationType, _lifetime, new List<Type>(){_serviceType}, spawner);
    }
}
#endif