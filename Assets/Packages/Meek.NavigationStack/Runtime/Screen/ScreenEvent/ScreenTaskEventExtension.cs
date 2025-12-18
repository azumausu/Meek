using System.Collections.Generic;

namespace Meek.NavigationStack
{
    public static class ScreenTaskEventExtension
    {
        public static async global::System.Threading.Tasks.Task InvokeAsync(
            this IList<ScreenTaskEvent> self,
            string eventName,
            StackNavigationContext context
        )
        {
            foreach (var entry in self)
            {
                if (entry.EventName != eventName) continue;
                await entry.Function.Invoke(context);
            }
        }
    }
}