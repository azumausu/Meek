using System.Threading.Tasks;

namespace Meek.NavigationStack.Debugs
{
    public class NavigationStackDebugMiddleware : IMiddleware
    {
        async ValueTask IMiddleware.InvokeAsync(NavigationContext context, NavigationDelegate next)
        {
            var stackNavigationContext = context as StackNavigationContext;

            RuntimeNavigationStackManager.Instance.FireScreenWillNavigate(stackNavigationContext);

            await next(context);

            RuntimeNavigationStackManager.Instance.FireScreenDidNavigate(stackNavigationContext);
        }
    }
}