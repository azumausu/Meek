using System;

namespace Meek.NavigationStack
{
    public static class ScreenEventHolderExtensionForNavigatorAnimation
    {
        public static void ViewWillOpen(this EventHolder self, Action action) => self.RegisterActionEvent(
            NavigatorAnimationScreenEvent.ViewWillOpen.ToString(),
            action
        );
        
        public static void ViewWillOpen(this EventHolder self, Func<global::System.Threading.Tasks.Task> function) => 
            self.RegisterTaskEvent(
                NavigatorAnimationScreenEvent.ViewWillOpen.ToString(), 
                function
                );
        
        public static void ViewDidOpen(this EventHolder self, Action action) => self.RegisterActionEvent(
            NavigatorAnimationScreenEvent.ViewDidOpen.ToString(),
            action
        );
        
        public static void ViewDidOpen(this EventHolder self, Func<global::System.Threading.Tasks.Task> function) => 
            self.RegisterTaskEvent(
                NavigatorAnimationScreenEvent.ViewDidOpen.ToString(), 
                function
            );

        public static void ViewWillClose(this EventHolder self, Action action) => self.RegisterActionEvent(
            NavigatorAnimationScreenEvent.ViewWillClose.ToString(),
            action
        );
        
        public static void ViewWillClose(this EventHolder self, Func<global::System.Threading.Tasks.Task> function) => 
            self.RegisterTaskEvent(
                NavigatorAnimationScreenEvent.ViewWillClose.ToString(), 
                function
            ); 
        
        public static void ViewDidClose(this EventHolder self, Action action) => self.RegisterActionEvent(
            NavigatorAnimationScreenEvent.ViewDidClose.ToString(),
            action
        );
        
        public static void ViewDidClose(this EventHolder self, Func<global::System.Threading.Tasks.Task> function) => 
            self.RegisterTaskEvent(
                NavigatorAnimationScreenEvent.ViewDidClose.ToString(), 
                function
            ); 
      
    }
}