using System;
using System.Threading.Tasks;

namespace Meek
{
    public class Navigator : INavigator
    {
        private readonly IScreenContainer _screenContainer;
        private readonly NavigationDelegate _application;

        public IScreenContainer ScreenContainer => _screenContainer;

        public Navigator(IServiceProvider serviceProvider, NavigationDelegate application)
        {
            _screenContainer = serviceProvider.GetService<IScreenContainer>();
            _application = application;
        }

        public ValueTask NavigateAsync(NavigationContext context)
        {
            return _application(context);
        }
    }
}