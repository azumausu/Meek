using System;
using System.Threading.Tasks;

namespace Meek.NavigationStack
{
    public class ScreenDispatchAsyncEvent
    {
        public string EventName { get; init; }

        public Func<object, Task<bool>> Function { get; init; }
    }
}