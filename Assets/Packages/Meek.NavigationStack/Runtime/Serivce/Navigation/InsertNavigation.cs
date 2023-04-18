using System;
using System.Threading.Tasks;

namespace Meek.NavigationStack
{
    public class InsertNavigation
    {
        private readonly StackNavigationService _stackNavigationService;
        private readonly InsertContext _context = new();

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
        
        public InsertNavigation NextScreenParameter(object nextScreenParameter)
        {
            _context.NextScreenParameter = nextScreenParameter;
            return this;
        }
        
        public InsertNavigation IsCrossFade(bool isCrossFade)
        {
            _context.IsCrossFade = isCrossFade;
            return this;
        }
        
        public InsertNavigation SkipAnimation(bool skipAnimation)
        {
            _context.SkipAnimation = skipAnimation;
            return this;
        }
    }
}