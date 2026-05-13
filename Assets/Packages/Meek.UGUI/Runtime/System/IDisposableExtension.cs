#if MEEK_ENABLE_UGUI
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Meek.UGUI
{
    internal static class IDisposableExtension
    {
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
    }
}
#endif