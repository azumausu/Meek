using System;
using Meek.NavigationStack;
using Meek.UGUI;

namespace Meek.MVP
{
    public class MVPApplicationOption
    {
        public Func<IServiceProvider, IContainerBuilder> ContainerBuilderFactory;

        public IInputLocker InputLocker;

        public IPrefabViewManager PrefabViewManager;

        /// <summary>
        /// nullable
        /// </summary>
        public IServiceProvider Parent = null;
    }
}