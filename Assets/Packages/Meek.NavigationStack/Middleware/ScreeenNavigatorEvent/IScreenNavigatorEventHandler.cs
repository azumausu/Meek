namespace Meek.NavigationStack
{
    public interface IScreenNavigatorEventHandler
    {
        void ScreenWillNavigate(NavigationContext context); 
        
        void ScreenDidNavigate(NavigationContext context);
    }
}