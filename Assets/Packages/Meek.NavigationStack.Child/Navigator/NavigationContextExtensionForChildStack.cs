using System;

namespace Meek.NavigationStack.Child
{
    public static class NavigationContextExtensionForChildStack
    {
        public static ChildStackNavigationContext ToChildStackNavigationContext(this NavigationContext context)
        {
            return context as ChildStackNavigationContext ?? throw new InvalidOperationException();
        }
    }
}