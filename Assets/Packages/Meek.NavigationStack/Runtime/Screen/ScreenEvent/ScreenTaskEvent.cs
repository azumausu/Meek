using System;

namespace Meek.NavigationStack
{
    public record ScreenTaskEvent
    {
        public string EventName { get; init; }

        public Func<StackNavigationContext, global::System.Threading.Tasks.Task> Function { get; init; }
    }
}