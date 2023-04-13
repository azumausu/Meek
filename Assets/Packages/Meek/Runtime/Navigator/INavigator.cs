using System;
using System.Threading.Tasks;

namespace Meek
{
    public interface INavigator
    {
        IScreenContainer ScreenContainer { get; }
        IServiceProvider ServiceProvider { get; }
        
        ValueTask NavigateAsync(NavigationContext context);
    }
}