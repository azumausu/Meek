namespace Meek.NavigationStack
{
    public interface IScreenNavigatorEventHandler
    {
        void ScreenWillNavigate(StackNavigationContext context);

        void ScreenDidNavigate(StackNavigationContext context);
    }
}