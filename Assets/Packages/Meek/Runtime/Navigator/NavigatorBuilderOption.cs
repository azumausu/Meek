using System;

namespace Meek
{
    public class NavigatorBuilderOption
    {
        public IContainerBuilder ContainerBuilder;

        private Type _screenContainer;
        
        public Type ScreenContainer
        {
            get => _screenContainer;
            set
            {
                value.AssertImplementation<IScreenContainer>();
                _screenContainer = value;
            }
        }
    }
}