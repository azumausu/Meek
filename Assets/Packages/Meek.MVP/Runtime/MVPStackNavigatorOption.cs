using System;
using Meek.NavigationStack;
using Meek.UGUI;

namespace Meek.MVP
{
    public class MVPStackNavigatorOption
    {
        public IContainerBuilder ContainerBuilder;

        public IInputLocker InputLocker;

        public IPrefabViewManager PrefabViewManager;

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