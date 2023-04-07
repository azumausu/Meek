using System;

namespace Meek
{
    public interface INavigatorBuilder
    {
        IServiceProvider ServiceProvider { get; }

        INavigator Build();

        INavigatorBuilder Use(Func<NavigationDelegate, NavigationDelegate> component);
    }
}