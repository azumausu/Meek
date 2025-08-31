using System.Threading;

namespace Meek.NavigationStack
{
    public class NavigationSharedSemaphore
    {
        public readonly SemaphoreSlim NavigationSemaphore = new SemaphoreSlim(1, 1);
    }
}