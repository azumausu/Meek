using System.Threading.Tasks;

namespace Meek.NavigationStack
{
    public class InputLockerMiddleware : IMiddleware
    {
        private readonly InputLockerOption _option;
        
        public InputLockerMiddleware(InputLockerOption option)
        {
            _option = option;
        }
        
        public async ValueTask InvokeAsync(NavigationContext context, NavigationDelegate next)
        {
            using var locker = _option.InputLocker.LockInput();
            
            await next(context);
        }
    }
}