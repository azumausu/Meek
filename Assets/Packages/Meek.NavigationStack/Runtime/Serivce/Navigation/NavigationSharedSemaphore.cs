using System.Threading;

namespace Meek.NavigationStack
{
    public class NavigationSharedSemaphore
    {
        /// <summary>
        /// Semaphore to control all navigation operations
        /// </summary>
        public readonly SemaphoreSlim NavigationSemaphore = new SemaphoreSlim(1, 1);
    }
}