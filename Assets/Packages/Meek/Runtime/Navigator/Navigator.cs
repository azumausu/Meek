using System.Threading.Tasks;

namespace Meek
{
    public class Navigator : INavigator
    {
        private readonly IScreenContainer _screenContainer;
        private readonly IServiceProvider _serviceProvider;
        private readonly NavigationDelegate _application;

        public IScreenContainer ScreenContainer => _screenContainer;
        public IServiceProvider ServiceProvider => _serviceProvider;
        
        public Navigator(IServiceProvider serviceProvider, NavigationDelegate application)
        {
            _screenContainer = serviceProvider.GetService<IScreenContainer>();
            _serviceProvider = serviceProvider;
            _application = application;
        }

        public ValueTask NavigateAsync(NavigationContext context)
        {
            return _application(context);
        } 
    }
}