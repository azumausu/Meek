using System;
using System.Threading.Tasks;

namespace Meek.NavigationStack
{
    public class ChangeWithPopNavigation
    {
        private readonly StackNavigationService _stackNavigationService;
        private readonly ChangeContext _changeContext = new();
        private readonly BackToNavigation _backToNavigation;

        public ChangeWithPopNavigation(StackNavigationService stackNavigationService, BackToNavigation backToNavigation)
        {
            _stackNavigationService = stackNavigationService;
            _backToNavigation = backToNavigation;
        }
        
        public Task ChangeWithPopAsync<TRemoveScreen, TScreen>() 
            where TRemoveScreen : IScreen
            where TScreen : IScreen
        {
            return ChangeWithPopAsync(typeof(TScreen), typeof(TRemoveScreen));
        }
        
        public async Task ChangeWithPopAsync(Type baseScreen, Type pushScreenClassType)
        {
            if (baseScreen.FullName == pushScreenClassType.FullName)
                throw new ArgumentException($"The same type cannot be specified for PopScreen and PushScreen");

            var afterPopScreen = _stackNavigationService.ScreenContainer.GetScreenAfter(baseScreen);
            await _stackNavigationService.InsertScreenBeforeAsync(afterPopScreen?.GetType(), pushScreenClassType, new InsertContext()
            {
                IsCrossFade = false,
                NextScreenParameter = _changeContext.NextScreenParameter,
                SkipAnimation = true
            });
            await _backToNavigation
                .UpdateSkipAnimation(_changeContext.SkipAnimation)
                .IsCrossFade(_changeContext.IsCrossFade)
                .BackToAsync(pushScreenClassType);
        } 
        
        public ChangeWithPopNavigation NextScreenParameter(object nextScreenParameter)
        {
            _changeContext.NextScreenParameter = nextScreenParameter;
            return this;
        }
        
        public ChangeWithPopNavigation IsCrossFade(bool isCrossFade)
        {
            _changeContext.IsCrossFade = isCrossFade;
            return this;
        }
        
        public ChangeWithPopNavigation SkipAnimation(bool skipAnimation)
        {
            _changeContext.SkipAnimation = skipAnimation;
            return this;
        } 
    }
}