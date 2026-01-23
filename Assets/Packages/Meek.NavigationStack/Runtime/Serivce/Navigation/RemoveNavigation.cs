using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable enable

namespace Meek.NavigationStack
{
    public class RemoveNavigation
    {
        protected readonly NavigationSharedSemaphore SharedSemaphore;
        protected readonly StackNavigationService StackNavigationService;
        protected readonly RemoveContext RemoveContext = new();
        protected object? Sender;

        public RemoveNavigation(StackNavigationService stackNavigationService, NavigationSharedSemaphore sharedSemaphore)
        {
            StackNavigationService = stackNavigationService;
            SharedSemaphore = sharedSemaphore;
        }

        [Obsolete("Please use RemoveForget<TScreen>")]
        public virtual void Remove<TScreen>() where TScreen : IScreen
        {
            RemoveAsync<TScreen>().Forget();
        }

        public virtual void RemoveForget<TScreen>() where TScreen : IScreen
        {
            RemoveAsync<TScreen>().Forget();
        }

        public virtual Task RemoveAsync<TScreen>() where TScreen : IScreen
        {
            return RemoveAsync(typeof(TScreen));
        }

        public virtual async Task RemoveAsync(IScreen screen)
        {
            await StackNavigationService.RemoveAsync(screen, RemoveContext);
        }

        public virtual async Task RemoveAsync(Type screenClassType)
        {
            await SharedSemaphore.NavigationSemaphore.WaitAsync();
            try
            {
                await StackNavigationService.RemoveAsync(screenClassType, RemoveContext);
            }
            finally
            {
                SharedSemaphore.NavigationSemaphore.Release();
            }
        }

        public RemoveNavigation IsCrossFade(bool isCrossFade)
        {
            RemoveContext.IsCrossFade = isCrossFade;
            return this;
        }

        public RemoveNavigation SkipAnimation(bool skipAnimation)
        {
            RemoveContext.SkipAnimation = skipAnimation;
            return this;
        }

        public RemoveNavigation CustomFeature(string key, object value)
        {
            if (RemoveContext.CustomFeatures == null)
            {
                RemoveContext.CustomFeatures = new Dictionary<string, object>();
            }

            RemoveContext.CustomFeatures[key] = value;
            return this;
        }

        public RemoveNavigation SetSender(object sender)
        {
            Sender = sender;
            return this;
        }
    }
}