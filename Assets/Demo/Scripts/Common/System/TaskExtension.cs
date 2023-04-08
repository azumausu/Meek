using System.Threading.Tasks;
using UnityEngine;

namespace Sample
{
    internal static class TaskExtension
    {
        public static void Forget(this System.Threading.Tasks.Task self)
        {
            self.ContinueWith(
                x =>
                {
                    Debug.LogError(x.Exception);
                },
                TaskContinuationOptions.OnlyOnFaulted
            );
        }
    }
}