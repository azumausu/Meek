using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.Pool;

namespace Meek.NavigationStack
{
    public class BackToNavigation
    {
        private readonly StackNavigationService _stackNavigationService;

        public bool IsCrossFade { get; private set; } = false;
        public bool SkipAnimation { get; private set; } = true;
        
        public BackToNavigation(StackNavigationService stackNavigationService)
        {
            _stackNavigationService = stackNavigationService;
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
                    IsCrossFade = IsCrossFade,
                    SkipAnimation = SkipAnimation,
                });
            }
            else
            {
                foreach (var screen in removeScreenList.Skip(1))
                    await _stackNavigationService.RemoveAsync(screen.GetType(), new RemoveContext());
                await _stackNavigationService.PopAsync(new PopContext()
                {
                    IsCrossFade = IsCrossFade,
                    SkipAnimation = SkipAnimation,
                });
            }

            ListPool<IScreen>.Release(removeScreenList);
        }
        
        
        public BackToNavigation UpdateIsCrossFade(bool isCrossFade)
        {
            IsCrossFade = isCrossFade;
            return this;
        }
        
        public BackToNavigation UpdateSkipAnimation(bool skipAnimation)
        {
            SkipAnimation = skipAnimation;
            return this;
        }   
    }
}