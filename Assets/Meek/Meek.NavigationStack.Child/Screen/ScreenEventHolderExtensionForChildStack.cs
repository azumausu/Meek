using System;

namespace Meek.NavigationStack.Child
{
    public static class ScreenEventHolderExtensionForChildStack
    {
        public static void ChildScreenWillOpen(this EventHolder self, Action action)
        {
            self.RegisterActionEvent(ChildStackNavigatorScreenEvent.ChildScreenWillOpen.ToString(), action);
        }
        
        public static void ChildScreenDidOpen(this EventHolder self, Action action)
        {
            self.RegisterActionEvent(ChildStackNavigatorScreenEvent.ChildScreenDidOpen.ToString(), action); 
        }

        public static void ChildScreenWillClose(this EventHolder self, Action action)
        {
            self.RegisterActionEvent(ChildStackNavigatorScreenEvent.ChildScreenWillClose.ToString(), action); 
        }

        public static void ChildScreenDidClose(this EventHolder self, Action action)
        {
            self.RegisterActionEvent(ChildStackNavigatorScreenEvent.ChildScreenDidClose.ToString(), action);
        }
        
        public static void ChildScreenWillOpen(this EventHolder self, Func<global::System.Threading.Tasks.Task> function)
        {
            self.RegisterTaskEvent(ChildStackNavigatorScreenEvent.ChildScreenWillOpen.ToString(), function);
        }
        
        public static void ChildScreenDidOpen(this EventHolder self, Func<global::System.Threading.Tasks.Task> function)
        {
            self.RegisterTaskEvent(ChildStackNavigatorScreenEvent.ChildScreenDidOpen.ToString(), function); 
        }

        public static void ChildScreenWillClose(this EventHolder self, Func<global::System.Threading.Tasks.Task> function)
        {
            self.RegisterTaskEvent(ChildStackNavigatorScreenEvent.ChildScreenWillClose.ToString(), function); 
        }

        public static void ChildScreenDidClose(this EventHolder self, Func<global::System.Threading.Tasks.Task> function)
        {
            self.RegisterTaskEvent(ChildStackNavigatorScreenEvent.ChildScreenDidClose.ToString(), function);
        }
    }
}