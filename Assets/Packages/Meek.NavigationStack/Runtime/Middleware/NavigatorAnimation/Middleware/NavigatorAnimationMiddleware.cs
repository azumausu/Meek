using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.Scripting;

namespace Meek.NavigationStack
{
    public class NavigatorAnimationMiddleware : IMiddleware
    {
        private readonly IScreenContainer _screenContainer;
        private readonly ICoroutineRunner _coroutineRunner;

        private List<INavigatorAnimationStrategy> _transitionAnimationModules;

        [Preserve]
        public NavigatorAnimationMiddleware(
            IEnumerable<INavigatorAnimationStrategy> navigatorAnimationStrategies,
            IScreenContainer screenContainer,
            ICoroutineRunner coroutineRunner
        )
        {
            _transitionAnimationModules = new List<INavigatorAnimationStrategy>(navigatorAnimationStrategies);
            _screenContainer = screenContainer;
            _coroutineRunner = coroutineRunner;
        }

        public async ValueTask InvokeAsync(NavigationContext context, NavigationDelegate next)
        {
            var stackContext = context.ToStackNavigationContext();

            await next(context);

            // ScreenUIWillClose
            if (stackContext.NavigatingSourceType == StackNavigationSourceType.Pop)
            {
                if (context.FromScreen is not StackScreen fromUIScreen) throw new InvalidOperationException();
                fromUIScreen.ScreenEventInvoker.Invoke(NavigatorAnimationScreenEvent.ViewWillClose);
                await fromUIScreen.ScreenEventInvoker.InvokeAsync(NavigatorAnimationScreenEvent.ViewWillClose);
            }

            if (stackContext.NavigatingSourceType == StackNavigationSourceType.Remove)
            {
                var removeScreen = context.GetFeatureValue<IScreen>(StackNavigationContextFeatureDefine.RemoveScreen);
                if (removeScreen is not StackScreen removeUIScreen) throw new InvalidOperationException();
                removeUIScreen.ScreenEventInvoker.Invoke(NavigatorAnimationScreenEvent.ViewWillClose);
                await removeUIScreen.ScreenEventInvoker.InvokeAsync(NavigatorAnimationScreenEvent.ViewWillClose);
            }

            // ScreenUIWillOpen
            if (stackContext.NavigatingSourceType == StackNavigationSourceType.Push)
            {
                if (context.ToScreen is not StackScreen toUIScreen) throw new InvalidOperationException();
                toUIScreen.ScreenEventInvoker.Invoke(NavigatorAnimationScreenEvent.ViewWillOpen);
                await toUIScreen.ScreenEventInvoker.InvokeAsync(NavigatorAnimationScreenEvent.ViewWillOpen);
            }

            if (stackContext.NavigatingSourceType == StackNavigationSourceType.Insert)
            {
                if (context.ToScreen is not StackScreen toUIScreen) throw new InvalidOperationException();
                toUIScreen.ScreenEventInvoker.Invoke(NavigatorAnimationScreenEvent.ViewWillOpen);
                await toUIScreen.ScreenEventInvoker.InvokeAsync(NavigatorAnimationScreenEvent.ViewWillOpen);
            }

            // 遷移Animation
            // -- DIに最後に登録されたものから探す
            var strategy = _transitionAnimationModules.LastOrDefault(x => x.IsValid(stackContext));
            if (strategy != null)
            {
                var tcs = new TaskCompletionSource<bool>();
                _coroutineRunner.StartCoroutineWithCallback(strategy.PlayAnimationRoutine(stackContext),
                    () => tcs.SetResult(true));
                await tcs.Task;
            }

            // ScreenUIDidOpen
            if (stackContext.NavigatingSourceType == StackNavigationSourceType.Push)
            {
                if (context.ToScreen is not StackScreen toUIScreen) throw new InvalidOperationException();
                toUIScreen.ScreenEventInvoker.Invoke(NavigatorAnimationScreenEvent.ViewDidOpen);
                await toUIScreen.ScreenEventInvoker.InvokeAsync(NavigatorAnimationScreenEvent.ViewDidOpen);
            }

            if (stackContext.NavigatingSourceType == StackNavigationSourceType.Insert)
            {
                if (context.ToScreen is not StackScreen toUIScreen) throw new InvalidOperationException();
                toUIScreen.ScreenEventInvoker.Invoke(NavigatorAnimationScreenEvent.ViewDidOpen);
                await toUIScreen.ScreenEventInvoker.InvokeAsync(NavigatorAnimationScreenEvent.ViewDidOpen);
            }

            // ScreenUIDidClose
            if (stackContext.NavigatingSourceType == StackNavigationSourceType.Pop)
            {
                if (context.FromScreen is not StackScreen fromUIScreen) throw new InvalidOperationException();
                fromUIScreen.ScreenEventInvoker.Invoke(NavigatorAnimationScreenEvent.ViewDidClose);
                await fromUIScreen.ScreenEventInvoker.InvokeAsync(NavigatorAnimationScreenEvent.ViewDidClose);
            }

            if (stackContext.NavigatingSourceType == StackNavigationSourceType.Remove)
            {
                var removeScreen = context.GetFeatureValue<IScreen>(StackNavigationContextFeatureDefine.RemoveScreen);
                if (removeScreen is not StackScreen removeUIScreen) throw new InvalidOperationException();
                removeUIScreen.ScreenEventInvoker.Invoke(NavigatorAnimationScreenEvent.ViewDidClose);
                await removeUIScreen.ScreenEventInvoker.InvokeAsync(NavigatorAnimationScreenEvent.ViewDidClose);
            }
        }
    }
}