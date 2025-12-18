using System;

namespace Meek.NavigationStack
{
    public record ScreenActionEvent
    {
        public string EventName { get; init; }

        public Action<StackNavigationContext> Action { get; init; }
    }
}