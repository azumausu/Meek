using System.Threading.Tasks;

namespace Meek.NavigationStack
{
    public class StackNavigator : INavigator
    {
        private readonly INavigator _internalNavigator;
       
        public IScreenContainer ScreenContainer => _internalNavigator.ScreenContainer;
        public IServiceProvider ServiceProvider => _internalNavigator.ServiceProvider;
        
        public StackNavigator(INavigator internalNavigator)
        {
            _internalNavigator = internalNavigator;
        }

        public ValueTask NavigateAsync(NavigationContext context)
        {
            context.AppServices = _internalNavigator.ServiceProvider;
            return _internalNavigator.NavigateAsync(context);
        }
    }
}