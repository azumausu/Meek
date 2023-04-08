using System;

namespace Meek.NavigationStack
{
    public record ScreenTaskEvent
    {
        public string EventName { get; init; }
        
        public Func<global::System.Threading.Tasks.Task> Function { get; init; } 
    }
}