using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.Pool;

namespace Meek.NavigationStack
{
    public class BackToNavigation
    {
        private readonly StackNavigationService _stackNavigationService;

        private bool _isCrossFade = false;
        private bool _skipAnimation = true;

        public BackToNavigation(StackNavigationService stackNavigationService)
        {
            _stackNavigationService = stackNavigationService;
        }

        public void BackTo<TBackScreen>() where TBackScreen : IScreen
        {
            BackToAsync<TBackScreen>().Forget();
        }

        public Task BackToAsync<TBackScreen>() where TBackScreen : IScreen
        {
            return BackToAsync(typeof(TBackScreen));
        }

        public async Task BackToAsync(Type backScreen)
        {
            ListPool<IScreen>.Get(out var removeScreenList);

            foreach (var screen in _stackNavigationService.ScreenContainer.Screens)
            {
                if (backScreen == screen.GetType()) break;
                removeScreenList.Add(screen);
            }

            if (removeScreenList.Count == 1)
            {
                await _stackNavigationService.PopAsync(new PopContext()
                {
                    IsCrossFade = _isCrossFade,
                    SkipAnimation = _skipAnimation,
                });
            }
            else
            {
                foreach (var screen in removeScreenList.Skip(1))
                    await _stackNavigationService.RemoveAsync(screen.GetType(), new RemoveContext());
                await _stackNavigationService.PopAsync(new PopContext()
                {
                    IsCrossFade = _isCrossFade,
                    SkipAnimation = _skipAnimation,
                });
            }

            ListPool<IScreen>.Release(removeScreenList);
        }


        public BackToNavigation IsCrossFade(bool isCrossFade)
        {
            _isCrossFade = isCrossFade;
            return this;
        }

        public BackToNavigation UpdateSkipAnimation(bool skipAnimation)
        {
            _skipAnimation = skipAnimation;
            return this;
        }
    }
}