using System;

namespace Meek.NavigationStack
{
    public static class ScreenViewEventHolderExtension
    {
        public static void ViewWillSetup(this EventHolder self, Action action) => self.RegisterActionEvent(
            ScreenViewEvent.ViewWillSetup.ToString(),
            action
        );
        
        public static void ViewDidSetup(this EventHolder self, Action action) => self.RegisterActionEvent(
            ScreenViewEvent.ViewDidSetup.ToString(),
            action
        );
    }
}