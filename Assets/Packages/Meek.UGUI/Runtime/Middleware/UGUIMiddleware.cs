#if MEEK_ENABLE_UGUI
using System;
using System.Linq;
using System.Threading.Tasks;
using Meek.NavigationStack;

namespace Meek.UGUI
{
    public class UGUIMiddleware : IMiddleware
    {
        private readonly IPrefabViewManager _prefabViewManager;
        
        public UGUIMiddleware(IPrefabViewManager prefabViewManager)
        {
            _prefabViewManager = prefabViewManager;
        }
        
        public async ValueTask InvokeAsync(NavigationContext context, NavigationDelegate next)
        {
            var stackContext = context as StackNavigationContext ?? throw new InvalidOperationException();
                
            await next(context);

            StackScreen stackScreen = null;
            if (stackContext.NavigatingSourceType == StackNavigationSourceType.Push)
            {
                if (context.ToScreen is not StackScreen toUIScreen) throw new InvalidOperationException();
                stackScreen = toUIScreen;
            }
            if (stackContext.NavigatingSourceType == StackNavigationSourceType.Insert)
            {
                var insertionScreen = stackContext.GetFeatureValue<IScreen>(StackNavigationContextFeatureDefine.InsertionScreen);
                if (insertionScreen is not StackScreen insertionUIScreen) throw new InvalidOperationException();
                stackScreen = insertionUIScreen;
            }

            
            if (stackScreen != null)
            {
                // Viewのロード時に呼び出すように修正したので一旦コメントアウト
                // foreach (var prefabViewHandler in stackScreen.UI.ViewHandlers.OfType<PrefabViewHandler>())
                //     _prefabViewManager.AddInHierarchy(prefabViewHandler);
                _prefabViewManager.SortOrderInHierarchy(stackContext);
            }
        }
    }
}
#endif