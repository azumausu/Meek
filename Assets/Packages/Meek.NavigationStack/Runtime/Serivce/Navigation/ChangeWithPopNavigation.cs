using System;
using System.Threading.Tasks;

namespace Meek.NavigationStack
{
    public class ChangeWithPopNavigation
    {
        private readonly StackNavigationService _stackNavigationService;
        private readonly ChangeContext _changeContext = new();
        private readonly BackToNavigation _backToNavigation;

        public object NextScreenParameter => _changeContext.NextScreenParameter;
        public bool IsCrossFade => _changeContext.IsCrossFade;
        public bool SkipAnimation => _changeContext.SkipAnimation;
        
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
                NextScreenParameter = NextScreenParameter,
                SkipAnimation = true
            });
            await _backToNavigation
                .UpdateSkipAnimation(SkipAnimation)
                .UpdateIsCrossFade(IsCrossFade)
                .BackToAsync(pushScreenClassType);
        } 
        
        public ChangeWithPopNavigation UpdateNextScreenParameter(object nextScreenParameter)
        {
            _changeContext.NextScreenParameter = nextScreenParameter;
            return this;
        }
        
        public ChangeWithPopNavigation UpdateIsCrossFade(bool isCrossFade)
        {
            _changeContext.IsCrossFade = isCrossFade;
            return this;
        }
        
        public ChangeWithPopNavigation UpdateSkipAnimation(bool skipAnimation)
        {
            _changeContext.SkipAnimation = skipAnimation;
            return this;
        } 
    }
}