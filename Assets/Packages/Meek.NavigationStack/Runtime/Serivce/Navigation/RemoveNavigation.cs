using System;
using System.Threading.Tasks;

namespace Meek.NavigationStack
{
    public class RemoveNavigation
    {
        private readonly StackNavigationService _stackNavigationService;
        private readonly RemoveContext _removeContext = new();

        public RemoveNavigation(StackNavigationService stackNavigationService)
        {
            _stackNavigationService = stackNavigationService;
        }
        
        public Task RemoveAsync<TScreen>() where TScreen : IScreen
        {
            return RemoveAsync(typeof(TScreen));
        }
        
        public Task RemoveAsync(Type screenClassType)
        {
            return _stackNavigationService.RemoveAsync(screenClassType, _removeContext);
        } 
        
        public RemoveNavigation IsCrossFade(bool isCrossFade)
        {
            _removeContext.IsCrossFade = isCrossFade;
            return this;
        }
        
        public RemoveNavigation SkipAnimation(bool skipAnimation)
        {
            _removeContext.SkipAnimation = skipAnimation;
            return this;
        }
    }
}