using System;
using Meek.NavigationStack;
using Meek.NavigationStack.Debugs;
using Meek.UGUI;

namespace Meek.MVP
{
    public class MvpNavigatorOptions
    {
        public IInputLocker InputLocker { get; init; }

        public IPrefabViewManager PrefabViewManager { get; init; }

        public NavigationStackDebugOption DebugOption { get; init; } = new();
    }
}