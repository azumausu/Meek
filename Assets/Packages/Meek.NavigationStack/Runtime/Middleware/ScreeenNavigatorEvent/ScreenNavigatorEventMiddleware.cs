using System.Threading;
using System.Threading.Tasks;

namespace Meek.NavigationStack
{
    /// <summary>
    /// 複数のトランジションが一気に行われることを回避する。
    /// </summary>
    public class ScreenNavigatorEventMiddleware : IMiddleware
    {
        private readonly SemaphoreSlim _navigationLock = new SemaphoreSlim(1, 1);

        public async ValueTask InvokeAsync(NavigationContext context, NavigationDelegate next)
        {
            await _navigationLock.WaitAsync();

            try
            {
                var stackContext = context.ToStackNavigationContext();
                var fromScreenEventHandler = context.FromScreen as IScreenNavigatorEventHandler;
                var toScreenEventHandler = context.ToScreen as IScreenNavigatorEventHandler;

                fromScreenEventHandler?.ScreenWillNavigate(stackContext);
                toScreenEventHandler?.ScreenWillNavigate(stackContext);

                try
                {
                    await next(context);
                }
                finally
                {
                    toScreenEventHandler?.ScreenDidNavigate(stackContext);
                    fromScreenEventHandler?.ScreenDidNavigate(stackContext);
                }
            }
            finally
            {
                _navigationLock.Release();
            }
        }
    }
}