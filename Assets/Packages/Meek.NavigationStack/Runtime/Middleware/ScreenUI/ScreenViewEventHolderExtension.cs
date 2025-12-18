using System;

namespace Meek.NavigationStack
{
    public static class ScreenViewEventHolderExtension
    {
        public static void ViewWillSetup(this EventHolder self, Action action) => self.RegisterActionEvent(
            nameof(ScreenViewEvent.ViewWillSetup),
            action
        );

        public static void ViewWillSetup(this EventHolder self, Action<StackNavigationContext> action) => self.RegisterActionEvent(
            nameof(ScreenViewEvent.ViewWillSetup),
            action
        );

        public static void ViewDidSetup(this EventHolder self, Action action) => self.RegisterActionEvent(
            nameof(ScreenViewEvent.ViewDidSetup),
            action
        );

        public static void ViewDidSetup(this EventHolder self, Action<StackNavigationContext> action) => self.RegisterActionEvent(
            nameof(ScreenViewEvent.ViewDidSetup),
            action
        );
    }
}