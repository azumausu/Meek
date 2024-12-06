using System.Threading.Tasks;

namespace Meek.NavigationStack
{
    public class InputLockerMiddleware : IMiddleware
    {
        private readonly IInputLocker _inputLocker;

        public InputLockerMiddleware(IInputLocker inputLocker)
        {
            _inputLocker = inputLocker;
        }

        public async ValueTask InvokeAsync(NavigationContext context, NavigationDelegate next)
        {
            using var locker = _inputLocker.LockInput();

            await next(context);
        }
    }
}