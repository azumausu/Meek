using System;
using System.Threading.Tasks;

namespace Meek.NavigationStack
{
    public class InsertNavigation
    {
        private readonly StackNavigationService _stackNavigationService;
        private readonly InsertContext _context = new();

        public object NextScreenParameter => _context.NextScreenParameter;
        public bool IsCrossFade => _context.IsCrossFade;
        public bool SkipAnimation => _context.SkipAnimation;
        
        public InsertNavigation(StackNavigationService stackNavigationService)
        {
            _stackNavigationService = stackNavigationService;
        }
        
        public Task InsertScreenBeforeAsync<TBeforeScreen, TInsertionScreen>()
            where TBeforeScreen : IScreen
            where TInsertionScreen : IScreen
        {
            return InsertScreenBeforeAsync(typeof(TBeforeScreen), typeof(TInsertionScreen));
        }
        
        public Task InsertScreenBeforeAsync(Type beforeScreenClassType, Type insertionScreenClassType)
        {
            return _stackNavigationService.InsertScreenBeforeAsync(beforeScreenClassType, insertionScreenClassType, this._context);
        }
        
        public InsertNavigation UpdateNextScreenParameter(object nextScreenParameter)
        {
            _context.NextScreenParameter = nextScreenParameter;
            return this;
        }
        
        public InsertNavigation UpdateIsCrossFade(bool isCrossFade)
        {
            _context.IsCrossFade = isCrossFade;
            return this;
        }
        
        public InsertNavigation UpdateSkipAnimation(bool skipAnimation)
        {
            _context.SkipAnimation = skipAnimation;
            return this;
        }
    }
}