#if UNITY_EDITOR
using System;

namespace Meek.NavigationStack.Debugs
{
    public class ServiceEntry
    {
        public string DisplayName { get; init; }

        public IServiceProvider ServiceProvider { get; init; }
    }
}
#endif