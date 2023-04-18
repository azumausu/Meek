using System;

namespace Meek
{
    public class NavigatorBuilderOption
    {
        public IContainerBuilder ContainerBuilder;

        private Type _screenNavigator;
        
        public Type ScreenNavigator
        {
            get => _screenNavigator;
            set
            {
                value.AssertImplementation<IScreenContainer>();
                _screenNavigator = value;
            }
        }
    }
}