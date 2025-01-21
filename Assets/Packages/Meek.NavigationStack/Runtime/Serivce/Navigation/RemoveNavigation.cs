using System;
using System.Threading.Tasks;

#nullable enable

namespace Meek.NavigationStack
{
    public class RemoveNavigation
    {
        protected readonly StackNavigationService StackNavigationService;
        protected readonly RemoveContext RemoveContext = new();
        protected object? Sender;

        public RemoveNavigation(StackNavigationService stackNavigationService)
        {
            StackNavigationService = stackNavigationService;
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

        public virtual Task RemoveAsync(Type screenClassType)
        {
            return StackNavigationService.RemoveAsync(screenClassType, RemoveContext);
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

        public RemoveNavigation SetSender(object sender)
        {
            Sender = sender;
            return this;
        }
    }
}