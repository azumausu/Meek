using System.Threading.Tasks;

namespace Meek.NavigationStack
{
    public class PopNavigation
    {
        private readonly StackNavigationService _stackNavigationService;
        private readonly PopContext _context = new();

        public bool IsCrossFade => _context.IsCrossFade;
        public bool SkipAnimation => _context.SkipAnimation;
        
        public PopNavigation(StackNavigationService stackNavigationService)
        {
            _stackNavigationService = stackNavigationService;
        }
        
        public Task PopAsync()
        {
            return _stackNavigationService.PopAsync(_context);
        } 
        
        public PopNavigation UpdateIsCrossFade(bool isCrossFade)
        {
            _context.IsCrossFade = isCrossFade;
            return this;
        }
        
        public PopNavigation UpdateSkipAnimation(bool skipAnimation)
        {
            _context.SkipAnimation = skipAnimation;
            return this;
        }
    }
}