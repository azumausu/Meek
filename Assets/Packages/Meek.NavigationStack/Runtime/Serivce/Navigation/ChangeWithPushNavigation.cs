using System;
using System.Threading.Tasks;

namespace Meek.NavigationStack
{
    public class ChangeWithPushNavigation
    { 
        private readonly StackNavigationService _stackNavigationService;
        private readonly ChangeContext _changeContext = new();

        public object NextScreenParameter => _changeContext.NextScreenParameter;
        public bool IsCrossFade => _changeContext.IsCrossFade;
        public bool SkipAnimation => _changeContext.SkipAnimation;
        
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
                IsCrossFade = IsCrossFade,
                NextScreenParameter = NextScreenParameter,
                SkipAnimation = SkipAnimation
            });

            foreach (var screen in _stackNavigationService.ScreenContainer.Screens)
            {
                await _stackNavigationService.RemoveAsync(screen.GetType(), new RemoveContext());
                if (baseScreen.FullName == screen.GetType().FullName) break;
            }
        } 
        
        public ChangeWithPushNavigation UpdateNextScreenParameter(object nextScreenParameter)
        {
            _changeContext.NextScreenParameter = nextScreenParameter;
            return this;
        }
        
        public ChangeWithPushNavigation UpdateIsCrossFade(bool isCrossFade)
        {
            _changeContext.IsCrossFade = isCrossFade;
            return this;
        }
        
        public ChangeWithPushNavigation UpdateSkipAnimation(bool skipAnimation)
        {
            _changeContext.SkipAnimation = skipAnimation;
            return this;
        }  
    }
}