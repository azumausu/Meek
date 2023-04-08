using System.Collections.Generic;
using System.Linq;

namespace Meek.NavigationStack
{
    public static class ScreenEventActionExtension
    {
        public static void Invoke(this IEnumerable<ScreenActionEvent> self, string eventName)
        {
            foreach (var entry in self.Where(x => x.EventName == eventName))
                entry.Action.Invoke();
        }
    }
}