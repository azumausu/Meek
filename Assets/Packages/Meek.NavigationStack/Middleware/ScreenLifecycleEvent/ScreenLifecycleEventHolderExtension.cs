using System;
using System.Runtime.CompilerServices;

namespace Meek.NavigationStack
{
    public static class ScreenLifecycleEventHolderExtension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ScreenWillStart(this EventHolder self, Action action) => self.RegisterActionEvent(
            ScreenLifecycleEvent.ScreenWillStart.ToString(),
            action
        );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ScreenWillStart(this EventHolder self, Func<global::System.Threading.Tasks.Task> function) => self.RegisterTaskEvent(
            ScreenLifecycleEvent.ScreenWillStart.ToString(),
            function
        );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ScreenDidStart(this EventHolder self, Action action) => self.RegisterActionEvent(
            ScreenLifecycleEvent.ScreenDidStart.ToString(),
            action
        );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ScreenWillPause(this EventHolder self, Action action) => self.RegisterActionEvent(
            ScreenLifecycleEvent.ScreenWillPause.ToString(),
            action
        );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ScreenDidPause(this EventHolder self, Action action) => self.RegisterActionEvent(
            ScreenLifecycleEvent.ScreenDidPause.ToString(),
            action
        );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ScreenDidPause(this EventHolder self, Func<global::System.Threading.Tasks.Task> function) => self.RegisterTaskEvent(
            ScreenLifecycleEvent.ScreenDidPause.ToString(),
            function
        );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ScreenWillResume(this EventHolder self, Action action) => self.RegisterActionEvent(
            ScreenLifecycleEvent.ScreenWillResume.ToString(),
            action
        );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ScreenWillResume(this EventHolder self, Func<global::System.Threading.Tasks.Task> function) => self.RegisterTaskEvent(
            ScreenLifecycleEvent.ScreenWillResume.ToString(),
            function
        );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ScreenDidResume(this EventHolder self, Action action) => self.RegisterActionEvent(
            ScreenLifecycleEvent.ScreenDidResume.ToString(),
            action
        );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ScreenWillDestroy(this EventHolder self, Action action) => self.RegisterActionEvent(
            ScreenLifecycleEvent.ScreenWillDestroy.ToString(), 
            action
        );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ScreenDidDestroy(this EventHolder self, Action action) => self.RegisterActionEvent(
            ScreenLifecycleEvent.ScreenDidDestroy.ToString(),
            action
        );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ScreenDidDestroy(this EventHolder self, Func<global::System.Threading.Tasks.Task> function) => self.RegisterTaskEvent(
            ScreenLifecycleEvent.ScreenDidDestroy.ToString(),
            function
        );

    }
}