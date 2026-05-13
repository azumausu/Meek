using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Meek.MVP
{
    internal static class IDisposableExtension
    {
        #region Methods

        public static void SafeDisposeAll(this IEnumerable<IDisposable> disposables)
        {
            foreach (var disposable in disposables)
            {
                disposable.SafeDispose();
            }
        }

        public static void SafeDispose(this IDisposable disposable)
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

        public static async ValueTask SafeDisposeAsync(this IAsyncDisposable disposable)
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

        #endregion
    }
}