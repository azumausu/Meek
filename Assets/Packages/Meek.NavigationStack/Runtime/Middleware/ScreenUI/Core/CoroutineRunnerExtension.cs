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
            if (ct.IsCancellationRequested) return Task.CompletedTask;
            
            var tcs = new TaskCompletionSource<bool>();
            ct.Register(() => tcs.SetCanceled());
            self.StartCoroutine(CoroutineWithCallbackInternal(action, () => tcs.SetResult(true)));
            
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
                self.StartCoroutineWithCallback(coroutine, () => invokeCount--);

            while (invokeCount > 0) yield return null; 
        }
        
        private static IEnumerator CoroutineWithCallbackInternal(IEnumerator action, Action onComplete)
        {
            yield return action;
            onComplete.Invoke(); 
        }
    }
}