using System;
using System.Threading.Tasks;

#nullable enable

namespace Meek.NavigationStack
{
    public class PushNavigation
    {
        protected readonly NavigationSharedSemaphore SharedSemaphore;
        protected readonly StackNavigationService StackNavigationService;
        protected readonly PushContext PushContext = new();
        protected object? Sender;

        public PushNavigation(StackNavigationService stackNavigationService, NavigationSharedSemaphore sharedSemaphore)
        {
            StackNavigationService = stackNavigationService;
            SharedSemaphore = sharedSemaphore;
        }

        [Obsolete("Please use PushForget<TScreen>")]
        public virtual void Push<TScreen>() where TScreen : IScreen
        {
            PushAsync<TScreen>().Forget();
        }

        public virtual void PushForget<TScreen>() where TScreen : IScreen
        {
            PushAsync<TScreen>().Forget();
        }

        public virtual Task PushAsync<TScreen>() where TScreen : IScreen
        {
            return PushAsync(typeof(TScreen));
        }

        public virtual async Task PushAsync(Type screenClassType)
        {
            await SharedSemaphore.NavigationSemaphore.WaitAsync();
            try
            {
                await StackNavigationService.PushAsync(screenClassType, PushContext);
            }
            finally
            {
                SharedSemaphore.NavigationSemaphore.Release();
            }
        }

        public PushNavigation NextScreenParameter(object nextScreenParameter)
        {
            PushContext.NextScreenParameter = nextScreenParameter;
            return this;
        }

        public PushNavigation IsCrossFade(bool isCrossFade)
        {
            PushContext.IsCrossFade = isCrossFade;
            return this;
        }

        public PushNavigation SkipAnimation(bool skipAnimation)
        {
            PushContext.SkipAnimation = skipAnimation;
            return this;
        }

        public PushNavigation SetSender(object sender)
        {
            Sender = sender;
            return this;
        }
    }
}