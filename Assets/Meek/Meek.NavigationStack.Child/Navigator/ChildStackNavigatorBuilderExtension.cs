namespace Meek.NavigationStack.Child
{
    public static class ChildStackNavigatorBuilderExtension
    {
        public static IServiceCollection AddChildStackNavigator(this IServiceCollection self)
        {
            
            return self;
        }
        
        public static INavigatorBuilder UseChildStackNavigator(this INavigatorBuilder self)
        {
            return self;
        }
    }
}