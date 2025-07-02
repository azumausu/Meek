using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Meek.NavigationStack
{
    internal static class IDisposableExtension
    {
        #region Methods

        public static void DisposeAll(this IEnumerable<IDisposable> disposables)
        {
            foreach (var disposable in disposables)
            {
                try
                {
                    disposable.Dispose();
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
        }

        public static async ValueTask DisposeAllAsync(this IEnumerable<IAsyncDisposable> disposables)
        {
            foreach (var disposable in disposables)
            {
                try
                {
                    await disposable.DisposeAsync();
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
        }

        #endregion
    }
}