using System.Threading.Tasks;

namespace Meek.NavigationStack
{
    public class ScreenLifecycleEventMiddleware : IMiddleware
    {
        public async ValueTask InvokeAsync(NavigationContext context, NavigationDelegate next)
        {
            var stackContext = context.ToStackNavigationContext();

            switch (stackContext.NavigatingSourceType)
            {
                case StackNavigationSourceType.Push:
                    await PushAsync(stackContext);
                    break;
                case StackNavigationSourceType.Pop:
                    await PopAsync(stackContext);
                    break;
                case StackNavigationSourceType.Remove:
                    await RemoveAsync(stackContext);
                    break;
                case StackNavigationSourceType.Insert:
                    await InsertAsync(stackContext);
                    break;
            }

            await next(context);
        }

        private async ValueTask PushAsync(StackNavigationContext context)
        {
            // Pause From Screen
            if (context.FromScreen is IScreenLifecycleEventHandler fromScreenEventHandler)
                await fromScreenEventHandler.PausingImplAsync(context);

            // Start To Screen
            if (context.ToScreen is IScreenLifecycleEventHandler toScreenEventHandler)
                await toScreenEventHandler.StartingImplAsync(context);
        }

        private async ValueTask PopAsync(StackNavigationContext context)
        {
            // Destroy From Screen
            if (context.FromScreen is IScreenLifecycleEventHandler fromScreenEventHandler)
                await fromScreenEventHandler.DestroyingImplAsync(context);

            // Resume To Screen
            if (context.ToScreen is IScreenLifecycleEventHandler toScreenEventHandler)
                await toScreenEventHandler.ResumingImplAsync(context);
        }

        private async ValueTask RemoveAsync(StackNavigationContext context)
        {
            // Destroy Remove Screen
            var removeScreen = context.GetRemoveScreen();
            if (removeScreen is IScreenLifecycleEventHandler removeScreenEventHandler)
                await removeScreenEventHandler.DestroyingImplAsync(context);
        }

        private async ValueTask InsertAsync(StackNavigationContext context)
        {
            // Start To Screen
            var insertionScreen = context.GetInsertionScreen();
            if (insertionScreen is IScreenLifecycleEventHandler eventHandler)
                await eventHandler.StartingImplAsync(context);

            // Pause To Screen
            if (insertionScreen is IScreenLifecycleEventHandler insertionScreenEventHandler)
                await insertionScreenEventHandler.PausingImplAsync(context);
        }
    }
}