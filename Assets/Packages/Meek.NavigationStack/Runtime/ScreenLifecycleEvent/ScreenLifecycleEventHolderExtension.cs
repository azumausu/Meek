using System;
using System.Runtime.CompilerServices;

namespace Meek.NavigationStack
{
    public static class ScreenLifecycleEventHolderExtension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ScreenWillStart(this EventHolder self, Action action) => self.RegisterActionEvent(
            nameof(ScreenLifecycleEvent.ScreenWillStart),
            action
        );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ScreenWillStart(this EventHolder self, Action<StackNavigationContext> action) => self.RegisterActionEvent(
            nameof(ScreenLifecycleEvent.ScreenWillStart),
            action
        );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ScreenWillStart(this EventHolder self, Func<global::System.Threading.Tasks.Task> function) =>
            self.RegisterTaskEvent(
                nameof(ScreenLifecycleEvent.ScreenWillStart),
                function
            );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ScreenWillStart(
            this EventHolder self,
            Func<StackNavigationContext, global::System.Threading.Tasks.Task> function
        ) =>
            self.RegisterTaskEvent(
                nameof(ScreenLifecycleEvent.ScreenWillStart),
                function
            );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ScreenDidStart(this EventHolder self, Action action) => self.RegisterActionEvent(
            nameof(ScreenLifecycleEvent.ScreenDidStart),
            action
        );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ScreenDidStart(this EventHolder self, Action<StackNavigationContext> action) => self.RegisterActionEvent(
            nameof(ScreenLifecycleEvent.ScreenDidStart),
            action
        );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ScreenWillPause(this EventHolder self, Action action) => self.RegisterActionEvent(
            nameof(ScreenLifecycleEvent.ScreenWillPause),
            action
        );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ScreenWillPause(this EventHolder self, Action<StackNavigationContext> action) => self.RegisterActionEvent(
            nameof(ScreenLifecycleEvent.ScreenWillPause),
            action
        );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ScreenDidPause(this EventHolder self, Action action) => self.RegisterActionEvent(
            nameof(ScreenLifecycleEvent.ScreenDidPause),
            action
        );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ScreenDidPause(this EventHolder self, Action<StackNavigationContext> action) => self.RegisterActionEvent(
            nameof(ScreenLifecycleEvent.ScreenDidPause),
            action
        );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ScreenDidPause(this EventHolder self, Func<global::System.Threading.Tasks.Task> function) =>
            self.RegisterTaskEvent(
                nameof(ScreenLifecycleEvent.ScreenDidPause),
                function
            );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ScreenDidPause(
            this EventHolder self,
            Func<StackNavigationContext, global::System.Threading.Tasks.Task> function
        ) =>
            self.RegisterTaskEvent(
                nameof(ScreenLifecycleEvent.ScreenDidPause),
                function
            );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ScreenWillResume(this EventHolder self, Action action) => self.RegisterActionEvent(
            nameof(ScreenLifecycleEvent.ScreenWillResume),
            action
        );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ScreenWillResume(this EventHolder self, Action<StackNavigationContext> action) => self.RegisterActionEvent(
            nameof(ScreenLifecycleEvent.ScreenWillResume),
            action
        );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ScreenWillResume(this EventHolder self, Func<global::System.Threading.Tasks.Task> function) =>
            self.RegisterTaskEvent(
                nameof(ScreenLifecycleEvent.ScreenWillResume),
                function
            );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ScreenWillResume(
            this EventHolder self,
            Func<StackNavigationContext, global::System.Threading.Tasks.Task> function
        ) =>
            self.RegisterTaskEvent(
                nameof(ScreenLifecycleEvent.ScreenWillResume),
                function
            );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ScreenDidResume(this EventHolder self, Action action) => self.RegisterActionEvent(
            nameof(ScreenLifecycleEvent.ScreenDidResume),
            action
        );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ScreenDidResume(this EventHolder self, Action<StackNavigationContext> action) => self.RegisterActionEvent(
            nameof(ScreenLifecycleEvent.ScreenDidResume),
            action
        );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ScreenWillDestroy(this EventHolder self, Action action) => self.RegisterActionEvent(
            nameof(ScreenLifecycleEvent.ScreenWillDestroy),
            action
        );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ScreenWillDestroy(this EventHolder self, Action<StackNavigationContext> action) => self.RegisterActionEvent(
            nameof(ScreenLifecycleEvent.ScreenWillDestroy),
            action
        );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ScreenDidDestroy(this EventHolder self, Action action) => self.RegisterActionEvent(
            nameof(ScreenLifecycleEvent.ScreenDidDestroy),
            action
        );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ScreenDidDestroy(this EventHolder self, Action<StackNavigationContext> action) => self.RegisterActionEvent(
            nameof(ScreenLifecycleEvent.ScreenDidDestroy),
            action
        );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ScreenDidDestroy(this EventHolder self, Func<global::System.Threading.Tasks.Task> function) =>
            self.RegisterTaskEvent(
                nameof(ScreenLifecycleEvent.ScreenDidDestroy),
                function
            );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ScreenDidDestroy(
            this EventHolder self,
            Func<StackNavigationContext, global::System.Threading.Tasks.Task> function
        ) =>
            self.RegisterTaskEvent(
                nameof(ScreenLifecycleEvent.ScreenDidDestroy),
                function
            );
    }
}