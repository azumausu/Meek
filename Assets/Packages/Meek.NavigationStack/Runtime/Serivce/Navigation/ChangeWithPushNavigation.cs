using System;
using System.Threading.Tasks;

namespace Meek.NavigationStack
{
    public class ChangeWithPushNavigation
    { 
        private readonly StackNavigationService _stackNavigationService;
        private readonly ChangeContext _changeContext = new();

        public ChangeWithPushNavigation(StackNavigationService stackNavigationService)
        {
            _stackNavigationService = stackNavigationService;
        }
        
        public Task ChangeWithPushAsync<TRemoveScreen, TScreen>() 
            where TRemoveScreen : IScreen
            where TScreen : IScreen
        {
            return ChangeWithPushAsync(typeof(TScreen), typeof(TRemoveScreen));
        }
        
        public async Task ChangeWithPushAsync(Type baseScreen, Type pushScreenClassType)
        {
            if (baseScreen.FullName == pushScreenClassType.FullName)
                throw new ArgumentException($"The same type cannot be specified for PopScreen and PushScreen");

            await _stackNavigationService.PushAsync(pushScreenClassType, new PushContext()
            {
                IsCrossFade = _changeContext.IsCrossFade,
                NextScreenParameter = _changeContext.NextScreenParameter,
                SkipAnimation = _changeContext.SkipAnimation
            });

            foreach (var screen in _stackNavigationService.ScreenContainer.Screens)
            {
                await _stackNavigationService.RemoveAsync(screen.GetType(), new RemoveContext());
                if (baseScreen.FullName == screen.GetType().FullName) break;
            }
        } 
        
        public ChangeWithPushNavigation NextScreenParameter(object nextScreenParameter)
        {
            _changeContext.NextScreenParameter = nextScreenParameter;
            return this;
        }
        
        public ChangeWithPushNavigation IsCrossFade(bool isCrossFade)
        {
            _changeContext.IsCrossFade = isCrossFade;
            return this;
        }
        
        public ChangeWithPushNavigation SkipAnimation(bool skipAnimation)
        {
            _changeContext.SkipAnimation = skipAnimation;
            return this;
        }  
    }
}