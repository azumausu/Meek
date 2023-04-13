using System;
using System.Threading.Tasks;

namespace Meek.NavigationStack
{
    public class PushNavigation
    {
        private readonly StackNavigationService _stackNavigationService;
        private readonly PushContext _pushContext = new();

        public object NextScreenParameter => _pushContext.NextScreenParameter;
        public bool IsCrossFade => _pushContext.IsCrossFade;
        public bool SkipAnimation => _pushContext.SkipAnimation;
        
        public PushNavigation(StackNavigationService stackNavigationService)
        {
            _stackNavigationService = stackNavigationService;
        }
        
        public Task PushAsync<TScreen>() where TScreen : IScreen
        {
            return PushAsync(typeof(TScreen));
        }
        
        public Task PushAsync(Type screenClassType)
        {
            return _stackNavigationService.PushAsync(screenClassType, _pushContext);
        } 
        
        public PushNavigation UpdateNextScreenParameter(object nextScreenParameter)
        {
            _pushContext.NextScreenParameter = nextScreenParameter;
            return this;
        }
        
        public PushNavigation UpdateIsCrossFade(bool isCrossFade)
        {
            _pushContext.IsCrossFade = isCrossFade;
            return this;
        }
        
        public PushNavigation UpdateSkipAnimation(bool skipAnimation)
        {
            _pushContext.SkipAnimation = skipAnimation;
            return this;
        }
    }
}