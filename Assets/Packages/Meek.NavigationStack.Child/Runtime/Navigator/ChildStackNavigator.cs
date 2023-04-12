using System.Threading.Tasks;

namespace Meek.NavigationStack.Child
{
    public class ChildStackNavigator : INavigator
    {
        private readonly INavigator _internalNavigator;

        public IServiceProvider ServiceProvider => _internalNavigator.ServiceProvider;
        public IScreenContainer ScreenContainer => _internalNavigator.ScreenContainer;
        
        public ChildStackNavigator(INavigator internalNavigator)
        {
            _internalNavigator = internalNavigator;
        }

        public ValueTask NavigateAsync(NavigationContext context)
        {
            return _internalNavigator.NavigateAsync(context);
        }
    }
}