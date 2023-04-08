using System.Threading.Tasks;
using UnityEngine;

namespace Meek.NavigationStack
{
    internal static class TaskExtension
    {
        public static void Forget(this global::System.Threading.Tasks.Task self)
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