using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

#nullable enable

namespace Meek.NavigationStack
{
    public class BackToNavigation
    {
        protected readonly StackNavigationService StackNavigationService;

        protected bool CrossFade = false;
        protected bool SkipAnimation = false;
        protected object? Sender;

        public BackToNavigation(StackNavigationService stackNavigationService)
        {
            StackNavigationService = stackNavigationService;
        }

        [Obsolete("Please use BackToForget<TBackScreen>")]
        public virtual void BackTo<TBackScreen>() where TBackScreen : IScreen
        {
            BackToAsync<TBackScreen>().Forget();
        }

        public virtual void BackToForget<TBackScreen>() where TBackScreen : IScreen
        {
            BackToAsync<TBackScreen>().Forget();
        }

        public virtual Task BackToAsync<TBackScreen>() where TBackScreen : IScreen
        {
            return BackToAsync(typeof(TBackScreen));
        }

        public virtual async Task BackToAsync(Type backScreen)
        {
            ListPool<IScreen>.Get(out var removeScreenList);

            bool existBackToScreen = false;
            foreach (var screen in StackNavigationService.ScreenContainer.Screens)
            {
                if (backScreen == screen.GetType())
                {
                    existBackToScreen = true;
                    break;
                }

                removeScreenList.Add(screen);
            }

            if (!existBackToScreen) return;

            if (removeScreenList.Count == 1)
            {
                await StackNavigationService.PopAsync(new PopContext()
                {
                    IsCrossFade = CrossFade,
                    SkipAnimation = SkipAnimation,
                });
            }
            else if (removeScreenList.Count > 1)
            {
                foreach (var screen in removeScreenList.Skip(1))
                    await StackNavigationService.RemoveAsync(screen.GetType(), new RemoveContext());
                await StackNavigationService.PopAsync(new PopContext()
                {
                    IsCrossFade = CrossFade,
                    SkipAnimation = SkipAnimation,
                });
            }

            ListPool<IScreen>.Release(removeScreenList);
        }


        public BackToNavigation IsCrossFade(bool isCrossFade)
        {
            CrossFade = isCrossFade;
            return this;
        }

        public BackToNavigation UpdateSkipAnimation(bool skipAnimation)
        {
            SkipAnimation = skipAnimation;
            return this;
        }

        public BackToNavigation SetSender(object sender)
        {
            Sender = sender;
            return this;
        }
    }
}