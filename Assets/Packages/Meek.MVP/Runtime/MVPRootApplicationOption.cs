using System;
using Meek.NavigationStack;
using Meek.UGUI;

namespace Meek.MVP
{
    public class MVPRootApplicationOption
    {
        public Func<IServiceProvider, IContainerBuilder> ContainerBuilderFactory;

        public IInputLocker InputLocker;

        public IPrefabViewManager PrefabViewManager;

        /// <summary>
        /// Default: PresenterLoaderFactoryFromResources.cs
        /// </summary>
        private Type _presenterLoaderFactoryType;

        public Type PresenterLoaderFactoryType
        {
            get => _presenterLoaderFactoryType;
            set
            {
                value.AssertImplementation<IPresenterLoaderFactory>();
                _presenterLoaderFactoryType = value;
            }
        }
    }
}