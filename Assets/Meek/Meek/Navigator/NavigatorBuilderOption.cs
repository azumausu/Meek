namespace Meek
{
    public class NavigatorBuilderOption
    {
        public IContainerBuilder ContainerBuilder;

        public readonly ServiceType<IScreenContainer> ScreenNavigator = new();
    }
}