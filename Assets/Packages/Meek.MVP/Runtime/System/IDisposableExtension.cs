using System;
using System.Collections.Generic;
using UnityEngine;

namespace Meek.MVP
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

        #endregion
    }
}