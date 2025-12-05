using System;
using System.Collections.Generic;
using UnityEngine;

namespace Meek.NavigationStack
{
    public static class ScreenEventActionExtension
    {
        public static void Invoke(this IEnumerable<ScreenActionEvent> self, string eventName, bool suppressException)
        {
            foreach (var entry in self)
            {
                if (entry.EventName != eventName) continue;

                try
                {
                    entry.Action.Invoke();
                }
                catch (Exception e)
                {
                    if (suppressException)
                    {
                        Debug.LogException(e);
                        continue;
                    }

                    throw;
                }
            }
        }
    }
}