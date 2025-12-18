using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Meek.NavigationStack;
using Meek.NavigationStack.Debugs;
using Meek.UGUI;

namespace Meek.MVP
{
    public class MvpNavigator : INavigator
    {
        private readonly SemaphoreSlim _navigationLock = new SemaphoreSlim(1, 1);
        private readonly MvpNavigatorOptions _options;
        private readonly IInputLocker _inputLocker;
        private readonly ICoroutineRunner _coroutineRunner;
        private readonly IPrefabViewManager _prefabViewManager;
        private readonly List<INavigatorAnimationStrategy> _transitionAnimationModules = new();

        public IScreenContainer ScreenContainer { get; }

        public MvpNavigator(
            MvpNavigatorOptions options,
            IScreenContainer screenContainer,
            IInputLocker inputLocker,
            ICoroutineRunner coroutineRunner,
            IPrefabViewManager prefabViewManager,
            PushNavigatorAnimationStrategy pushNavigatorAnimationStrategy,
            PopNavigatorAnimationStrategy popNavigatorAnimationStrategy,
            InsertNavigatorAnimationStrategy insertNavigatorAnimationStrategy,
            RemoveNavigatorAnimationStrategy removeNavigatorAnimationStrategy
        )
        {
            _options = options;
            _inputLocker = inputLocker;
            _coroutineRunner = coroutineRunner;
            _prefabViewManager = prefabViewManager;
            _transitionAnimationModules.Add(pushNavigatorAnimationStrategy);
            _transitionAnimationModules.Add(popNavigatorAnimationStrategy);
            _transitionAnimationModules.Add(insertNavigatorAnimationStrategy);
            _transitionAnimationModules.Add(removeNavigatorAnimationStrategy);
            ScreenContainer = screenContainer;
        }

        public virtual async ValueTask NavigateAsync(NavigationContext context)
        {
            await _navigationLock.WaitAsync();

            var stackContext = context.ToStackNavigationContext();
            var fromScreenEventHandler = stackContext.FromScreen as IScreenNavigatorEventHandler;
            var toScreenEventHandler = stackContext.ToScreen as IScreenNavigatorEventHandler;

            if (_options.DebugOption.UseDebug)
            {
                RuntimeNavigationStackManager.Instance.FireScreenWillNavigate(stackContext);
            }

            fromScreenEventHandler?.ScreenWillNavigate(stackContext);
            toScreenEventHandler?.ScreenWillNavigate(stackContext);

            try
            {
                try
                {
                    using var locker = _inputLocker.LockInput();

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
                }
                finally
                {
                    toScreenEventHandler?.ScreenDidNavigate(stackContext);
                    fromScreenEventHandler?.ScreenDidNavigate(stackContext);

                    if (_options.DebugOption.UseDebug)
                    {
                        RuntimeNavigationStackManager.Instance.FireScreenDidNavigate(stackContext);
                    }
                }
            }
            finally
            {
                _navigationLock.Release();
            }
        }

        protected virtual async ValueTask PushAsync(StackNavigationContext context)
        {
            var toScreen = context.ToScreen as StackScreen ?? throw new InvalidOperationException();

            // === ScreenUI Middleware ===
            context.ToScreen?.Initialize(context);


            // === Screen Lifecycle Event ===
            // Pause From Screen
            if (context.FromScreen is IScreenLifecycleEventHandler fromScreenEventHandler)
            {
                await fromScreenEventHandler.PausingImplAsync(context);
            }

            // Start To Screen
            if (context.ToScreen is IScreenLifecycleEventHandler toScreenEventHandler)
            {
                await toScreenEventHandler.StartingImplAsync(context);
            }

            // === UpdateScreenContainer ===
            await ScreenContainer.NavigateAsync(context);

            // === UGUI Middleware ===
            _prefabViewManager.SortOrderInHierarchy(context);


            // ==== Animation Middleware ====
            toScreen.ScreenEventInvoker.Invoke(NavigatorAnimationScreenEvent.ViewWillOpen, context);
            await toScreen.ScreenEventInvoker.InvokeAsync(NavigatorAnimationScreenEvent.ViewWillOpen, context);
            await TransitionAnimationAsync(context);
            toScreen.ScreenEventInvoker.Invoke(NavigatorAnimationScreenEvent.ViewDidOpen, context);
            await toScreen.ScreenEventInvoker.InvokeAsync(NavigatorAnimationScreenEvent.ViewDidOpen, context);
        }

        protected virtual async ValueTask PopAsync(StackNavigationContext context)
        {
            var fromScreen = context.FromScreen as StackScreen ?? throw new InvalidOperationException();


            // === Screen Lifecycle Event ===
            // Destroy From Screen
            if (context.FromScreen is IScreenLifecycleEventHandler fromScreenEventHandler)
            {
                await fromScreenEventHandler.DestroyingImplAsync(context);
            }

            // Resume To Screen
            if (context.ToScreen is IScreenLifecycleEventHandler toScreenEventHandler)
            {
                await toScreenEventHandler.ResumingImplAsync(context);
            }

            // === UpdateScreenContainer ===
            await ScreenContainer.NavigateAsync(context);


            // ==== Animation Middleware ====
            fromScreen.ScreenEventInvoker.Invoke(NavigatorAnimationScreenEvent.ViewWillClose, context);
            await fromScreen.ScreenEventInvoker.InvokeAsync(NavigatorAnimationScreenEvent.ViewWillClose, context);
            await TransitionAnimationAsync(context);
            fromScreen.ScreenEventInvoker.Invoke(NavigatorAnimationScreenEvent.ViewDidClose, context);
            await fromScreen.ScreenEventInvoker.InvokeAsync(NavigatorAnimationScreenEvent.ViewDidClose, context);


            // === ScreenUIMiddleware ===
            if (context.FromScreen is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync();
            }

            if (context.FromScreen is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        protected virtual async ValueTask RemoveAsync(StackNavigationContext context)
        {
            var removeScreen = context.GetRemoveScreen() ?? throw new InvalidOperationException();


            // === Screen Lifecycle Event ===
            // Destroy Remove Screen
            if (removeScreen is IScreenLifecycleEventHandler removeScreenEventHandler)
            {
                await removeScreenEventHandler.DestroyingImplAsync(context);
            }

            // === UpdateScreenContainer ===
            await ScreenContainer.NavigateAsync(context);


            // === Animation Middleware ====
            removeScreen.ScreenEventInvoker.Invoke(NavigatorAnimationScreenEvent.ViewWillClose, context);
            await removeScreen.ScreenEventInvoker.InvokeAsync(NavigatorAnimationScreenEvent.ViewWillClose, context);
            await TransitionAnimationAsync(context);
            removeScreen.ScreenEventInvoker.Invoke(NavigatorAnimationScreenEvent.ViewDidClose, context);
            await removeScreen.ScreenEventInvoker.InvokeAsync(NavigatorAnimationScreenEvent.ViewDidClose, context);


            // === ScreenUIMiddleware ===
            if (removeScreen is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync();
            }

            if (removeScreen is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        protected virtual async ValueTask InsertAsync(StackNavigationContext context)
        {
            var toScreen = context.ToScreen as StackScreen ?? throw new InvalidOperationException();
            var insertionScreen = context.GetInsertionScreen() ?? throw new InvalidOperationException();


            // === ScreenUI Middleware ===
            ((IScreen)insertionScreen).Initialize(context);

            // === Screen Lifecycle Event ===
            // Start To Screen
            if (insertionScreen is IScreenLifecycleEventHandler eventHandler)
            {
                await eventHandler.StartingImplAsync(context);
            }

            // Pause To Screen
            if (insertionScreen is IScreenLifecycleEventHandler insertionScreenEventHandler)
            {
                await insertionScreenEventHandler.PausingImplAsync(context);
            }

            // === UpdateScreenContainer ===
            await ScreenContainer.NavigateAsync(context);


            // === UGUI Middleware ===
            _prefabViewManager.SortOrderInHierarchy(context);


            // ==== Animation Middleware ====
            toScreen.ScreenEventInvoker.Invoke(NavigatorAnimationScreenEvent.ViewWillOpen, context);
            await toScreen.ScreenEventInvoker.InvokeAsync(NavigatorAnimationScreenEvent.ViewWillOpen, context);
            await TransitionAnimationAsync(context);
            toScreen.ScreenEventInvoker.Invoke(NavigatorAnimationScreenEvent.ViewDidOpen, context);
            await toScreen.ScreenEventInvoker.InvokeAsync(NavigatorAnimationScreenEvent.ViewDidOpen, context);
        }

        private async ValueTask TransitionAnimationAsync(StackNavigationContext context)
        {
            var strategy = _transitionAnimationModules.LastOrDefault(x => x.IsValid(context));
            if (strategy != null)
            {
                var tcs = new TaskCompletionSource<bool>();
                _coroutineRunner.StartCoroutineWithCallback(strategy.PlayAnimationRoutine(context), () => tcs.SetResult(true));
                await tcs.Task;
            }
        }
    }
}