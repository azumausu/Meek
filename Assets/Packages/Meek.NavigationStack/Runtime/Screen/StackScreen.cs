using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace Meek.NavigationStack
{
    public abstract class StackScreen : IScreen, IDisposable, IAsyncDisposable, IScreenLifecycleEventHandler, IScreenNavigatorEventHandler
    {
        private readonly Stack<IDisposable> _interactableLocks = new Stack<IDisposable>();

        protected readonly List<IDisposable> Disposables = new List<IDisposable>();
        protected readonly List<IAsyncDisposable> AsyncDisposables = new List<IAsyncDisposable>();

        protected IServiceProvider AppServices;

        private ICoroutineRunner _coroutineRunner;

        public ScreenUI UI { get; private set; }
        public IScreenEventInvoker ScreenEventInvoker { get; private set; }

        protected virtual PushNavigation PushNavigation => AppServices.GetService<PushNavigation>().SetSender(this);
        protected virtual PopNavigation PopNavigation => AppServices.GetService<PopNavigation>().SetSender(this);
        protected virtual RemoveNavigation RemoveNavigation => AppServices.GetService<RemoveNavigation>().SetSender(this);
        protected virtual InsertNavigation InsertNavigation => AppServices.GetService<InsertNavigation>().SetSender(this);
        protected virtual BackToNavigation BackToNavigation => AppServices.GetService<BackToNavigation>().SetSender(this);

        protected virtual void Dispatch<TParam>(TParam param) => AppServices.GetService<StackNavigationService>().Dispatch(param);
        protected virtual Task DispatchAsync<TParam>(TParam param) => AppServices.GetService<StackNavigationService>().DispatchAsync(param);
        public StackNavigationService NavigationService => AppServices.GetService<StackNavigationService>();

        /// <summary>
        ///     StateType
        /// </summary>
        public virtual ScreenUIType ScreenUIType => ScreenUIType.FullScreen;

        protected abstract void RegisterEventsInternal(EventHolder eventHolder);

        protected virtual void StateWillNavigate(StackNavigationContext context)
        {
            if (context.NavigatingSourceType is StackNavigationSourceType.Insert or StackNavigationSourceType.Remove)
                return;

            if (context.NavigatingSourceType == StackNavigationSourceType.Pop)
                ScreenEventInvoker.Invoke(ScreenLifecycleEvent.ScreenWillDestroy);

            if (context.NavigatingSourceType == StackNavigationSourceType.Push)
            {
                ScreenEventInvoker.Invoke(ScreenLifecycleEvent.ScreenWillPause);
                _interactableLocks.Push(UI.LockInteractable());
            }
        }

        protected virtual async ValueTask StartingImplAsync(StackNavigationContext context)
        {
            // 初期化
            ScreenEventInvoker = CreateEventInvoker();

            // lockをかける
            _interactableLocks.Push(UI.LockInteractable());

            ScreenEventInvoker.Invoke(ScreenLifecycleEvent.ScreenWillStart);
            await ScreenEventInvoker.InvokeAsync(ScreenLifecycleEvent.ScreenWillStart);

            // Load完了を待ってSetup処理の実行
            var tcs = new TaskCompletionSource<bool>();
            if (UI.IsLoaded) tcs.SetResult(true);
            else
            {
                var waitUntil = new WaitUntil(() => UI.IsLoaded);
                _coroutineRunner.StartCoroutineWithCallback(waitUntil, () => tcs.SetResult(true));
            }

            await tcs.Task;

            UI.SetLayer(context);
            ScreenEventInvoker.Invoke(ScreenViewEvent.ViewWillSetup);
            UI.Setup(context);
            ScreenEventInvoker.Invoke(ScreenViewEvent.ViewDidSetup);
        }

        protected virtual async ValueTask ResumingImplAsync(StackNavigationContext context)
        {
            ScreenEventInvoker.Invoke(ScreenLifecycleEvent.ScreenWillResume);
            await ScreenEventInvoker.InvokeAsync(ScreenLifecycleEvent.ScreenWillResume);
        }

        protected virtual async ValueTask PausingImplAsync(StackNavigationContext context)
        {
            ScreenEventInvoker.Invoke(ScreenLifecycleEvent.ScreenDidPause);
            await ScreenEventInvoker.InvokeAsync(ScreenLifecycleEvent.ScreenDidPause);
        }

        protected virtual async ValueTask DestroyingImplAsync(StackNavigationContext context)
        {
            ScreenEventInvoker.Invoke(ScreenLifecycleEvent.ScreenDidDestroy);
            await ScreenEventInvoker.InvokeAsync(ScreenLifecycleEvent.ScreenDidDestroy);
            _interactableLocks?.DisposeAll();
            _interactableLocks?.Clear();
        }

        protected virtual void StateDidNavigate(StackNavigationContext context)
        {
            if (context.NavigatingSourceType is StackNavigationSourceType.Insert or StackNavigationSourceType.Remove)
                return;

            // 一致している場合はStateDidStartかStateDidResumeになる
            var stateEvent = context.NavigatingSourceType switch
            {
                StackNavigationSourceType.Push => ScreenLifecycleEvent.ScreenDidStart,
                StackNavigationSourceType.Pop => ScreenLifecycleEvent.ScreenDidResume,
                _ => throw new ArgumentOutOfRangeException()
            };

            _interactableLocks.Pop().Dispose();
            ScreenEventInvoker.Invoke(stateEvent);
        }

        private IScreenEventInvoker CreateEventInvoker()
        {
            var eventHolder = new EventHolder();
            RegisterEventsInternal(eventHolder);
            return eventHolder;
        }

        void IScreen.Initialize(NavigationContext navigationContext)
        {
            var stackContext = navigationContext.ToStackNavigationContext();

            AppServices = stackContext.AppServices;
            _coroutineRunner = stackContext.AppServices.GetService<ICoroutineRunner>();
            UI = stackContext.AppServices.GetService<ScreenUI>();
        }

        ValueTask IScreenLifecycleEventHandler.StartingImplAsync(NavigationContext context)
        {
            var stackContext = context.ToStackNavigationContext();
            return StartingImplAsync(stackContext);
        }

        ValueTask IScreenLifecycleEventHandler.ResumingImplAsync(NavigationContext context)
        {
            var stackContext = context.ToStackNavigationContext();
            return ResumingImplAsync(stackContext);
        }

        ValueTask IScreenLifecycleEventHandler.PausingImplAsync(NavigationContext context)
        {
            var stackContext = context.ToStackNavigationContext();
            return PausingImplAsync(stackContext);
        }

        ValueTask IScreenLifecycleEventHandler.DestroyingImplAsync(NavigationContext context)
        {
            var stackContext = context.ToStackNavigationContext();
            return DestroyingImplAsync(stackContext);
        }

        void IScreenNavigatorEventHandler.ScreenWillNavigate(NavigationContext context)
        {
            var stackContext = context.ToStackNavigationContext();
            StateWillNavigate(stackContext);
        }

        void IScreenNavigatorEventHandler.ScreenDidNavigate(NavigationContext context)
        {
            var stackContext = context.ToStackNavigationContext();
            StateDidNavigate(stackContext);
        }

        [CanBeNull]
        public TScreen TryGetScreen<TScreen>() where TScreen : class, IScreen
        {
            var navigationService = NavigationService;

            foreach (var screen in navigationService.ScreenContainer.Screens)
            {
                if (screen is TScreen tScreen)
                {
                    return tScreen;
                }
            }

            return null;
        }

        public virtual void Dispose()
        {
            Disposables.DisposeAll();
            Disposables.Clear();
        }

        public virtual async ValueTask DisposeAsync()
        {
            await UI.DisposeAsync();
            foreach (var disposable in AsyncDisposables)
            {
                await disposable.DisposeAsync();
            }

            AsyncDisposables.Clear();
        }
    }
}