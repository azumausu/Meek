using System;

namespace Meek.NavigationStack
{
    public record ScreenDispatchEvent
    {
        public string EventName { get; init; }
        
        public Func<object, bool> Function { get; init; } 
    }
}