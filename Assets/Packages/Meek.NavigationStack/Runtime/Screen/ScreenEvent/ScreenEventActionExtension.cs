using System.Collections.Generic;

namespace Meek.NavigationStack
{
    public static class ScreenEventActionExtension
    {
        public static void Invoke(this IEnumerable<ScreenActionEvent> self, string eventName)
        {
            foreach (var entry in self)
            {
                if (entry.EventName != eventName) continue;

                entry.Action.Invoke();
            }
        }
    }
}