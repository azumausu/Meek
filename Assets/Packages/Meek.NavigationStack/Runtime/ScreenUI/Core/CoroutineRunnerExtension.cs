using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Meek.NavigationStack
{
    public static class CoroutineRunnerExtension
    {
        public static Task StartCoroutineAsTask(this ICoroutineRunner self, IEnumerator action, CancellationToken ct = default)
        {
            if (ct.IsCancellationRequested) return Task.FromCanceled(ct);

            var tcs = new TaskCompletionSource<bool>();
            var registration = ct.Register(() => tcs.TrySetCanceled(ct));
            self.StartCoroutine(CoroutineWithCallbackInternal(action, () =>
            {
                registration.Dispose();
                tcs.TrySetResult(true);
            }));

            return tcs.Task;
        }

        public static IEnumerator StartParallelCoroutine(this ICoroutineRunner self, IReadOnlyCollection<IEnumerator> coroutines)
        {
            return self.StartParallelCoroutineInternal(coroutines, coroutines.Count);
        }

        public static void StartCoroutineWithCallback(this ICoroutineRunner self, IEnumerator action, Action onComplete)
        {
            self.StartCoroutine(CoroutineWithCallbackInternal(action, onComplete));
        }

        private static IEnumerator StartParallelCoroutineInternal(
            this ICoroutineRunner self,
            IEnumerable<IEnumerator> coroutines,
            int length
        )
        {
            var invokeCount = length;
            foreach (var coroutine in coroutines)
            {
                self.StartCoroutineWithCallback(coroutine, () => invokeCount--);
            }

            while (invokeCount > 0) yield return null;
        }

        private static IEnumerator CoroutineWithCallbackInternal(IEnumerator action, Action onComplete)
        {
            try
            {
                yield return action;
            }
            finally
            {
                onComplete?.Invoke();
            }
        }
    }
}