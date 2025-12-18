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

        private readonly List<INavigatorAnimationStrategy> _transitionAnimationModules = new();

        [Preserve]
        public NavigatorAnimationMiddleware(
            IScreenContainer screenContainer,
            ICoroutineRunner coroutineRunner,
            PushNavigatorAnimationStrategy pushNavigatorAnimationStrategy,
            PopNavigatorAnimationStrategy popNavigatorAnimationStrategy,
            RemoveNavigatorAnimationStrategy removeNavigatorAnimationStrategy,
            InsertNavigatorAnimationStrategy insertNavigatorAnimationStrategy
        )
        {
            _screenContainer = screenContainer;
            _coroutineRunner = coroutineRunner;

            _transitionAnimationModules.Add(pushNavigatorAnimationStrategy);
            _transitionAnimationModules.Add(popNavigatorAnimationStrategy);
            _transitionAnimationModules.Add(removeNavigatorAnimationStrategy);
            _transitionAnimationModules.Add(insertNavigatorAnimationStrategy);
        }

        public async ValueTask InvokeAsync(NavigationContext context, NavigationDelegate next)
        {
            var stackContext = context.ToStackNavigationContext();

            await next(context);

            // ScreenUIWillClose
            if (stackContext.NavigatingSourceType == StackNavigationSourceType.Pop)
            {
                if (context.FromScreen is not StackScreen fromUIScreen) throw new InvalidOperationException();
                fromUIScreen.ScreenEventInvoker.Invoke(NavigatorAnimationScreenEvent.ViewWillClose, stackContext);
                await fromUIScreen.ScreenEventInvoker.InvokeAsync(NavigatorAnimationScreenEvent.ViewWillClose, stackContext);
            }

            if (stackContext.NavigatingSourceType == StackNavigationSourceType.Remove)
            {
                var removeScreen = stackContext.GetRemoveScreen();
                removeScreen.ScreenEventInvoker.Invoke(NavigatorAnimationScreenEvent.ViewWillClose, stackContext);
                await removeScreen.ScreenEventInvoker.InvokeAsync(NavigatorAnimationScreenEvent.ViewWillClose, stackContext);
            }

            // ScreenUIWillOpen
            if (stackContext.NavigatingSourceType == StackNavigationSourceType.Push)
            {
                if (context.ToScreen is not StackScreen toUIScreen) throw new InvalidOperationException();
                toUIScreen.ScreenEventInvoker.Invoke(NavigatorAnimationScreenEvent.ViewWillOpen, stackContext);
                await toUIScreen.ScreenEventInvoker.InvokeAsync(NavigatorAnimationScreenEvent.ViewWillOpen, stackContext);
            }

            if (stackContext.NavigatingSourceType == StackNavigationSourceType.Insert)
            {
                if (context.ToScreen is not StackScreen toUIScreen) throw new InvalidOperationException();
                toUIScreen.ScreenEventInvoker.Invoke(NavigatorAnimationScreenEvent.ViewWillOpen, stackContext);
                await toUIScreen.ScreenEventInvoker.InvokeAsync(NavigatorAnimationScreenEvent.ViewWillOpen, stackContext);
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
                toUIScreen.ScreenEventInvoker.Invoke(NavigatorAnimationScreenEvent.ViewDidOpen, stackContext);
                await toUIScreen.ScreenEventInvoker.InvokeAsync(NavigatorAnimationScreenEvent.ViewDidOpen, stackContext);
            }

            if (stackContext.NavigatingSourceType == StackNavigationSourceType.Insert)
            {
                if (context.ToScreen is not StackScreen toUIScreen) throw new InvalidOperationException();
                toUIScreen.ScreenEventInvoker.Invoke(NavigatorAnimationScreenEvent.ViewDidOpen, stackContext);
                await toUIScreen.ScreenEventInvoker.InvokeAsync(NavigatorAnimationScreenEvent.ViewDidOpen, stackContext);
            }

            // ScreenUIDidClose
            if (stackContext.NavigatingSourceType == StackNavigationSourceType.Pop)
            {
                if (context.FromScreen is not StackScreen fromUIScreen) throw new InvalidOperationException();
                fromUIScreen.ScreenEventInvoker.Invoke(NavigatorAnimationScreenEvent.ViewDidClose, stackContext);
                await fromUIScreen.ScreenEventInvoker.InvokeAsync(NavigatorAnimationScreenEvent.ViewDidClose, stackContext);
            }

            if (stackContext.NavigatingSourceType == StackNavigationSourceType.Remove)
            {
                var removeScreen = stackContext.GetRemoveScreen();
                removeScreen.ScreenEventInvoker.Invoke(NavigatorAnimationScreenEvent.ViewDidClose, stackContext);
                await removeScreen.ScreenEventInvoker.InvokeAsync(NavigatorAnimationScreenEvent.ViewDidClose, stackContext);
            }
        }
    }
}