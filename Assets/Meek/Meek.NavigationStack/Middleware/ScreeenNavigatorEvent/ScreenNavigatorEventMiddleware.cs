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
                // 遷移処理開始
                if (context.FromScreen is IScreenNavigatorEventHandler fromScreenEventHandler)
                    fromScreenEventHandler.ScreenWillNavigate(context);

                await next(context);

                // 遷移処理完了
                if (context.ToScreen is IScreenNavigatorEventHandler toScreenEventHandler)
                    toScreenEventHandler.ScreenDidNavigate(context);
            }
            finally { _navigationLock.Release(); }
        }
    }
}