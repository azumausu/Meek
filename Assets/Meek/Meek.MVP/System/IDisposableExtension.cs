using System;
using System.Collections.Generic;

namespace Meek.MVP
{
    internal static class IDisposableExtension
    {
        #region Methods

        public static void DisposeAll(this IEnumerable<IDisposable> disposables)
        {
            foreach (var disposable in disposables) disposable.Dispose();
        }

        #endregion
    }
}