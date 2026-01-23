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

        public virtual async Task<TScreen> PushAsync<TScreen>() where TScreen : IScreen
        {
            var screen = await PushAsync(typeof(TScreen));
            return (TScreen)screen;
        }

        public virtual async Task<IScreen> PushAsync(Type screenClassType)
        {
            await SharedSemaphore.NavigationSemaphore.WaitAsync();
            try
            {
                return await StackNavigationService.PushAsync(screenClassType, PushContext);
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

        public PushNavigation CustomFeature(string key, object value)
        {
            if (PushContext.CustomFeatures == null)
            {
                PushContext.CustomFeatures = new System.Collections.Generic.Dictionary<string, object>();
            }

            PushContext.CustomFeatures[key] = value;
            return this;
        }

        public PushNavigation SetSender(object sender)
        {
            Sender = sender;
            return this;
        }
    }
}