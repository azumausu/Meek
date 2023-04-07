using System.Threading.Tasks;

namespace Meek.NavigationStack.Child
{
    public class SyncChildScreenContainerMiddleware : IMiddleware
    {
        private readonly INavigator _childStackNavigator;
        
        public SyncChildScreenContainerMiddleware(INavigator childStackNavigator)
        {
            _childStackNavigator = childStackNavigator;
        }
        
        public async ValueTask InvokeAsync(NavigationContext context, NavigationDelegate next)
        {
            var stackNavigationContext = context.ToStackNavigationContext();
            
            await next(context);

            if (stackNavigationContext.NavigatingSourceType is StackNavigationSourceType.Pop)
            {
                var popScreen = context.FromScreen as StackScreen;
                var childScreenContainer = _childStackNavigator.ScreenContainer as ChildStackScreenContainer;
                childScreenContainer.RemoveChildScreenByParent(popScreen);
            }
            
            if (stackNavigationContext.NavigatingSourceType is StackNavigationSourceType.Remove)
            {
                var removeScreen = context.GetFeatureValue<StackScreen>(StackNavigationContextFeatureDefine.RemoveScreen);
                var childScreenContainer = _childStackNavigator.ScreenContainer as ChildStackScreenContainer;
                childScreenContainer.RemoveChildScreenByParent(removeScreen);
            }
        }
    }
}