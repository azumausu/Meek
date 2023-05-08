using System;
using System.Threading.Tasks;

namespace Meek.NavigationStack
{
    public class PushNavigation
    {
        private readonly StackNavigationService _stackNavigationService;
        private readonly PushContext _pushContext = new();

        public PushNavigation(StackNavigationService stackNavigationService)
        {
            _stackNavigationService = stackNavigationService;
        }

        public void Push<TScreen>() where TScreen : IScreen
        {
            PushAsync<TScreen>().Forget();
        }

        public Task PushAsync<TScreen>() where TScreen : IScreen
        {
            return PushAsync(typeof(TScreen));
        }

        public Task PushAsync(Type screenClassType)
        {
            return _stackNavigationService.PushAsync(screenClassType, _pushContext);
        }

        public PushNavigation NextScreenParameter(object nextScreenParameter)
        {
            _pushContext.NextScreenParameter = nextScreenParameter;
            return this;
        }

        public PushNavigation IsCrossFade(bool isCrossFade)
        {
            _pushContext.IsCrossFade = isCrossFade;
            return this;
        }

        public PushNavigation SkipAnimation(bool skipAnimation)
        {
            _pushContext.SkipAnimation = skipAnimation;
            return this;
        }
    }
}