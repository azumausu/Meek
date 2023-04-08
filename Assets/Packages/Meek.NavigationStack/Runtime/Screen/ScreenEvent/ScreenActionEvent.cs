using System;

namespace Meek.NavigationStack
{
    public record ScreenActionEvent
    {
        public string EventName { get; init; }
        
        public Action Action { get; init; } 
    }
}