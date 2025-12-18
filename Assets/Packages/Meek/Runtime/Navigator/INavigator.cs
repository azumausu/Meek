using System;
using System.Threading.Tasks;

namespace Meek
{
    public interface INavigator
    {
        IScreenContainer ScreenContainer { get; }

        ValueTask NavigateAsync(NavigationContext context);
    }
}