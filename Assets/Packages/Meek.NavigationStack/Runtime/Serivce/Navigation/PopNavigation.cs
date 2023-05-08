using System.Threading.Tasks;

namespace Meek.NavigationStack
{
    public class PopNavigation
    {
        private readonly StackNavigationService _stackNavigationService;
        private readonly PopContext _context = new();

        public PopNavigation(StackNavigationService stackNavigationService)
        {
            _stackNavigationService = stackNavigationService;
        }

        public void Pop()
        {
            _stackNavigationService.PopAsync(_context).Forget();
        }

        public Task PopAsync()
        {
            return _stackNavigationService.PopAsync(_context);
        }

        public PopNavigation IsCrossFade(bool isCrossFade)
        {
            _context.IsCrossFade = isCrossFade;
            return this;
        }

        public PopNavigation SkipAnimation(bool skipAnimation)
        {
            _context.SkipAnimation = skipAnimation;
            return this;
        }
    }
}