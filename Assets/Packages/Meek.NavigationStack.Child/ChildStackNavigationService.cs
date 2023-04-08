using System.Linq;
using System.Threading.Tasks;
using UnityEngine.Pool;

namespace Meek.NavigationStack.Child
{
    public class ChildStackNavigationService
    {
        private readonly INavigator _childStackNavigator;
        private readonly IServiceProvider _serviceProvider;
        
        public ChildStackNavigationService(INavigator childStackNavigator, IServiceProvider serviceProvider)
        {
            _childStackNavigator = childStackNavigator;
            _serviceProvider = serviceProvider;
        }
        
        public async Task PushAsync<TChildScreen>(StackScreen parentScreen)
            where TChildScreen : IChildScreen
        {
            DictionaryPool<string, object>.Get(out var features);

            var childScreen = _serviceProvider.GetService<TChildScreen>();
            var childPeekScreen = _childStackNavigator.ScreenContainer.Screens
                .OfType<IChildScreen>()
                .FirstOrDefault(x => x.ParentScreen == parentScreen);
            
            var context = new ChildStackNavigationContext()
            {
                NavigationSourceType = ChildStackNavigationSourceType.Push,
                ParentScreen = parentScreen,
                FromScreen = (IScreen)childPeekScreen ?? parentScreen,
                ToScreen = childScreen,
                Features = features,
                AppServices = _serviceProvider,
            };
            
            await _childStackNavigator.NavigateAsync(context);
            
            DictionaryPool<string, object>.Release(features);
        }
        
        public async Task PopAsync(StackScreen parentScreen)
        {
            DictionaryPool<string, object>.Get(out var features);
            
            var fromScreen = _childStackNavigator.ScreenContainer.Screens
                .OfType<IChildScreen>()
                .First(x => x.ParentScreen == parentScreen);
            var toScreen = _childStackNavigator.ScreenContainer.Screens
                .OfType<IChildScreen>()
                .Skip(1)
                .FirstOrDefault(x => x.ParentScreen == parentScreen); 
            
            var context = new ChildStackNavigationContext()
            {
                NavigationSourceType = ChildStackNavigationSourceType.Pop,
                ParentScreen = parentScreen,
                FromScreen = fromScreen,
                ToScreen = (IScreen)toScreen ?? parentScreen,
                Features = features,
                AppServices = _serviceProvider,
            };
            
            await _childStackNavigator.NavigateAsync(context);
            
            DictionaryPool<string, object>.Release(features);
        }
    }
}