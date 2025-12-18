using System;

namespace Meek.NavigationStack
{
    public static class ScreenEventHolderExtensionForNavigatorAnimation
    {
        public static void ViewWillOpen(this EventHolder self, Action action) => self.RegisterActionEvent(
            nameof(NavigatorAnimationScreenEvent.ViewWillOpen),
            action
        );

        public static void ViewWillOpen(this EventHolder self, Action<StackNavigationContext> action) => self.RegisterActionEvent(
            nameof(NavigatorAnimationScreenEvent.ViewWillOpen),
            action
        );

        public static void ViewWillOpen(this EventHolder self, Func<global::System.Threading.Tasks.Task> function) =>
            self.RegisterTaskEvent(
                nameof(NavigatorAnimationScreenEvent.ViewWillOpen),
                function
            );

        public static void ViewWillOpen(
            this EventHolder self,
            Func<StackNavigationContext, global::System.Threading.Tasks.Task> function
        ) =>
            self.RegisterTaskEvent(
                nameof(NavigatorAnimationScreenEvent.ViewWillOpen),
                function
            );

        public static void ViewDidOpen(this EventHolder self, Action action) => self.RegisterActionEvent(
            nameof(NavigatorAnimationScreenEvent.ViewDidOpen),
            action
        );

        public static void ViewDidOpen(this EventHolder self, Action<StackNavigationContext> action) => self.RegisterActionEvent(
            nameof(NavigatorAnimationScreenEvent.ViewDidOpen),
            action
        );

        public static void ViewDidOpen(this EventHolder self, Func<global::System.Threading.Tasks.Task> function) =>
            self.RegisterTaskEvent(
                nameof(NavigatorAnimationScreenEvent.ViewDidOpen),
                function
            );

        public static void ViewDidOpen(this EventHolder self, Func<StackNavigationContext, global::System.Threading.Tasks.Task> function) =>
            self.RegisterTaskEvent(
                nameof(NavigatorAnimationScreenEvent.ViewDidOpen),
                function
            );

        public static void ViewWillClose(this EventHolder self, Action action) => self.RegisterActionEvent(
            NavigatorAnimationScreenEvent.ViewWillClose.ToString(),
            action
        );

        public static void ViewWillClose(this EventHolder self, Action<StackNavigationContext> action) => self.RegisterActionEvent(
            nameof(NavigatorAnimationScreenEvent.ViewWillClose),
            action
        );

        public static void ViewWillClose(this EventHolder self, Func<global::System.Threading.Tasks.Task> function) =>
            self.RegisterTaskEvent(
                nameof(NavigatorAnimationScreenEvent.ViewWillClose),
                function
            );

        public static void ViewWillClose(
            this EventHolder self,
            Func<StackNavigationContext, global::System.Threading.Tasks.Task> function
        ) =>
            self.RegisterTaskEvent(
                nameof(NavigatorAnimationScreenEvent.ViewWillClose),
                function
            );

        public static void ViewDidClose(this EventHolder self, Action action) => self.RegisterActionEvent(
            nameof(NavigatorAnimationScreenEvent.ViewDidClose),
            action
        );

        public static void ViewDidClose(this EventHolder self, Action<StackNavigationContext> action) => self.RegisterActionEvent(
            nameof(NavigatorAnimationScreenEvent.ViewDidClose),
            action
        );

        public static void ViewDidClose(this EventHolder self, Func<global::System.Threading.Tasks.Task> function) =>
            self.RegisterTaskEvent(
                nameof(NavigatorAnimationScreenEvent.ViewDidClose),
                function
            );

        public static void ViewDidClose(
            this EventHolder self,
            Func<StackNavigationContext, global::System.Threading.Tasks.Task> function
        ) =>
            self.RegisterTaskEvent(
                nameof(NavigatorAnimationScreenEvent.ViewDidClose),
                function
            );
    }
}