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

            // ここで、nextを呼び出した方が直感的かを検討する
            // next()

            // Start To Screen
            if (context.ToScreen is IScreenLifecycleEventHandler toScreenEventHandler)
                await toScreenEventHandler.StartingImplAsync(context);
        }

        private async ValueTask PopAsync(StackNavigationContext context)
        {
            // Destroy From Screen
            if (context.FromScreen is IScreenLifecycleEventHandler fromScreenEventHandler)
                await fromScreenEventHandler.DestroyingImplAsync(context);

            // ここで、nextを呼び出した方が直感的かを検討する
            // next()

            // Resume To Screen
            if (context.ToScreen is IScreenLifecycleEventHandler toScreenEventHandler)
                await toScreenEventHandler.ResumingImplAsync(context);
        }

        private async ValueTask RemoveAsync(StackNavigationContext context)
        {
            // Destroy Remove Screen
            var removeScreen = context.GetFeatureValue<IScreen>(StackNavigationContextFeatureDefine.RemoveScreen);
            if (removeScreen is IScreenLifecycleEventHandler removeScreenEventHandler)
                await removeScreenEventHandler.DestroyingImplAsync(context);

            // ここで、nextを呼び出した方が直感的かを検討する
            // next()

            // この動作を入れたほうが直感的かを検討する
            // var removeScreenType = context.GetFeatureValue<Type>(StackNavigationContextFeatureDefine.RemoveScreenType);
            // var resumeScreen = _screenContainer.GetScreenAfter(removeScreenType);
            // if (resumeScreen is IScreenLifecycleEventHandler eventHandler) 
            //     await eventHandler.ResumingImplAsync(context); 
        }

        private async ValueTask InsertAsync(StackNavigationContext context)
        {
            // この動作を入れたほうが直感的かを検討する
            // Pause Insertion Before Screen
            // var pauseScreenType =
            //     context.GetFeatureValue<Type>(StackNavigationContextFeatureDefine.InsertionBeforeScreenType);
            // var pauseScreen = _screenContainer.GetScreen(pauseScreenType) ?? throw new InvalidOperationException("");
            // if (pauseScreen is IScreenLifecycleEventHandler pauseScreenEventHandler) 
            //     await pauseScreenEventHandler.PausingImplAsync(context); 

            // ここで、nextを呼び出した方が直感的かを検討する
            // next()

            // Start To Screen
            var insertionScreen = context.GetFeatureValue<IScreen>(StackNavigationContextFeatureDefine.InsertionScreen);
            if (insertionScreen is IScreenLifecycleEventHandler eventHandler)
                await eventHandler.StartingImplAsync(context);

            // Pause To Screen
            if (insertionScreen is IScreenLifecycleEventHandler insertionScreenEventHandler)
                await insertionScreenEventHandler.PausingImplAsync(context);
        }
    }
}