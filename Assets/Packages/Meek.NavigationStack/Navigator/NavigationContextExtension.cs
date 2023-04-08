using System;

namespace Meek.NavigationStack
{
    public static class NavigationContextExtension
    {
        public static StackNavigationContext ToStackNavigationContext(this NavigationContext context)
        {
            return context as StackNavigationContext ?? throw new InvalidOperationException();
        }
    }
}