using System;
using System.Threading;

namespace Meek.NavigationStack
{
    public class NavigationSharedSemaphore : IDisposable
    {
        /// <summary>
        /// Semaphore to control all navigation operations
        /// </summary>
        public readonly SemaphoreSlim NavigationSemaphore = new SemaphoreSlim(1, 1);

        public void Dispose()
        {
            NavigationSemaphore.Dispose();
        }
    }
}