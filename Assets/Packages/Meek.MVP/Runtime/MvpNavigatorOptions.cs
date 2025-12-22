using Meek.NavigationStack;
using Meek.UGUI;

namespace Meek.MVP
{
    public class MvpNavigatorOptions
    {
        /// <summary>
        /// Input locker to use when navigating between screens.
        /// </summary>
        public IInputLocker InputLocker { get; init; }

        /// <summary>
        /// Prefab view manager to use when navigating between screens.
        /// </summary>
        public IPrefabViewManager PrefabViewManager { get; init; }
    }
}