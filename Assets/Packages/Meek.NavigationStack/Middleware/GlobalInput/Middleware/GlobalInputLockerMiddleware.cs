using System.Threading.Tasks;

namespace Meek.NavigationStack
{
    public class GlobalInputLockerMiddleware : IMiddleware
    {
        private readonly GlobalInputLockerOption _option;
        
        public GlobalInputLockerMiddleware(GlobalInputLockerOption option)
        {
            _option = option;
        }
        
        public async ValueTask InvokeAsync(NavigationContext context, NavigationDelegate next)
        {
            using var locker = _option.GlobalInputLocker.LockInput();
            
            await next(context);
        }
    }
}