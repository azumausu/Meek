using System;
using System.Threading.Tasks;
using UnityEngine.Scripting;

namespace Meek.NavigationStack
{
    public class ScreenLifecycleEventMiddleware : IMiddleware
    {
        private readonly IScreenContainer _screenContainer;
        
        [Preserve]
        public ScreenLifecycleEventMiddleware(IScreenContainer screenContainer)
        {
            _screenContainer = screenContainer;
        }
        
        public async ValueTask InvokeAsync(NavigationContext context, NavigationDelegate next)
        {
            var stackContext = context.ToStackNavigationContext();
            // --- Pause処理: Active状態なScreenの上にScreenが乗った場合
            if (stackContext.NavigatingSourceType == StackNavigationSourceType.Push)
            {
                if (context.FromScreen is IScreenLifecycleEventHandler eventHandler) 
                    await eventHandler.PausingImplAsync(context); 
            }
            if (stackContext.NavigatingSourceType == StackNavigationSourceType.Insert)
            {
                var pauseScreenType =
                    context.GetFeatureValue<Type>(StackNavigationContextFeatureDefine.InsertionBeforeScreenType);
                var pauseScreen = _screenContainer.GetScreen(pauseScreenType) ?? throw new InvalidOperationException("");
                var insertionScreen = context.GetFeatureValue<IScreen>(StackNavigationContextFeatureDefine.InsertionScreen);
                if (pauseScreen is IScreenLifecycleEventHandler pauseScreenEventHandler) 
                    await pauseScreenEventHandler.PausingImplAsync(context); 
                if (insertionScreen is IScreenLifecycleEventHandler insertionScreenEventHandler) 
                    await insertionScreenEventHandler.PausingImplAsync(context); 
            }

            // --- Screen破棄処理
            if (stackContext.NavigatingSourceType == StackNavigationSourceType.Pop)
            {
                // Popの場合はFromScreenが破棄対象なので、必ずFromScreenが存在する
                if (context.FromScreen is IScreenLifecycleEventHandler eventHandler)
                    await eventHandler.DestroyingImplAsync(context);
            }
            if (stackContext.NavigatingSourceType == StackNavigationSourceType.Remove)
            {
                var removeScreen = context.GetFeatureValue<IScreen>(StackNavigationContextFeatureDefine.RemoveScreen);
                if (removeScreen is IScreenLifecycleEventHandler eventHandler)
                    await eventHandler.DestroyingImplAsync(context);
            }
            
            // 遷移処理開始
            // --- Screen作成処理: 新しく再生されるScreen
            if (stackContext.NavigatingSourceType == StackNavigationSourceType.Push)
            {
                if (context.ToScreen is IScreenLifecycleEventHandler eventHandler) 
                    await eventHandler.StartingImplAsync(context);
            }
            if (stackContext.NavigatingSourceType == StackNavigationSourceType.Insert)
            {
                var insertionScreen = context.GetFeatureValue<IScreen>(StackNavigationContextFeatureDefine.InsertionScreen);
                if (insertionScreen is IScreenLifecycleEventHandler eventHandler) 
                    await eventHandler.StartingImplAsync(context);
            }

            // --- Resume: 既に存在するステートに遷移する場合
            if (stackContext.NavigatingSourceType == StackNavigationSourceType.Pop)
            {
                if (context.ToScreen is IScreenLifecycleEventHandler eventHandler) 
                    await eventHandler.ResumingImplAsync(context);
            }
            if (stackContext.NavigatingSourceType == StackNavigationSourceType.Remove)
            {
                var removeScreenType = context.GetFeatureValue<Type>(StackNavigationContextFeatureDefine.RemoveScreenType);
                var resumeScreen = _screenContainer.GetScreenAfter(removeScreenType);
                if (resumeScreen is IScreenLifecycleEventHandler eventHandler) 
                    await eventHandler.ResumingImplAsync(context);
            }

            await next(context);
        }
    }
}