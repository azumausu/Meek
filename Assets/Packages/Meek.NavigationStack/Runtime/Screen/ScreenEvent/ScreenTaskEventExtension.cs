using System.Collections.Generic;
using System.Linq;

namespace Meek.NavigationStack
{
    public static class ScreenTaskEventExtension
    {
        public static async global::System.Threading.Tasks.Task InvokeAsync(this IEnumerable<ScreenTaskEvent> self, string eventName)
        {
            foreach (var entry in self.Where(x => x.EventName == eventName))
                await entry.Function.Invoke();
        }
    }
}