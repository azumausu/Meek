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
        private ICoroutineRunner _coroutineRunner;

        protected readonly List<IDisposable> Disposables = new List<IDisposable>();
        protected readonly List<IAsyncDisposable> AsyncDisposables = new List<IAsyncDisposable>();
        protected IServiceProvider AppServices;


        public ScreenUI UI { get; private set; }
        public IScreenEventInvoker ScreenEventInvoker { get; private set; }
        public StackNavigationService NavigationService => AppServices.GetService<StackNavigationService>();

        protected virtual PushNavigation PushNavigation => AppServices.GetService<PushNavigation>().SetSender(this);
        protected virtual PopNavigation PopNavigation => AppServices.GetService<PopNavigation>().SetSender(this);
        protected virtual RemoveNavigation RemoveNavigation => AppServices.GetService<RemoveNavigation>().SetSender(this);
        protected virtual InsertNavigation InsertNavigation => AppServices.GetService<InsertNavigation>().SetSender(this);
        protected virtual BackToNavigation BackToNavigation => AppServices.GetService<BackToNavigation>().SetSender(this);

        protected virtual void Dispatch<TParam>(TParam param) => AppServices.GetService<StackNavigationService>().Dispatch(param);
        protected virtual Task DispatchAsync<TParam>(TParam param) => AppServices.GetService<StackNavigationService>().DispatchAsync(param);

        /// <summary>
        /// When set to true, it will automatically call _interactableLocks.Dispose() when the Screen is destroyed.
        /// However, since the lock is briefly released during destruction, there is a risk that input may become temporarily enabled.
        /// </summary>
        protected virtual bool AutoDisposeLockerOnDestroy => false;

        public virtual ScreenUIType ScreenUIType => ScreenUIType.FullScreen;

        protected abstract void RegisterEventsInternal(EventHolder eventHolder);

        protected virtual async ValueTask StartingImplAsync(StackNavigationContext context)
        {
            ScreenEventInvoker = CreateEventInvoker();

            _interactableLocks.Push(UI.LockInteractable());

            ScreenEventInvoker.Invoke(ScreenLifecycleEvent.ScreenWillStart);
            await ScreenEventInvoker.InvokeAsync(ScreenLifecycleEvent.ScreenWillStart);

            var tcs = new TaskCompletionSource<bool>();
            if (UI.IsLoaded) tcs.SetResult(true);
            else
            {
                var waitUntil = new WaitUntil(() => UI.IsLoaded);
                _coroutineRunner.StartCoroutineWithCallback(waitUntil, () => tcs.SetResult(true));
            }

            await tcs.Task;

            UI.SetOpenAniationStartTime(context);
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

            if (AutoDisposeLockerOnDestroy)
            {
                ForceUnlockInteractable();
            }

            _interactableLocks?.Clear();
        }

        protected virtual void ScreenWillNavigate(StackNavigationContext context)
        {
            if (context.NavigatingSourceType is StackNavigationSourceType.Insert or StackNavigationSourceType.Remove)
            {
                return;
            }

            if (context.FromScreen == this && context.NavigatingSourceType == StackNavigationSourceType.Pop)
            {
                ScreenEventInvoker.Invoke(ScreenLifecycleEvent.ScreenWillDestroy);
            }

            if (context.FromScreen == this && context.NavigatingSourceType == StackNavigationSourceType.Push)
            {
                ScreenEventInvoker.Invoke(ScreenLifecycleEvent.ScreenWillPause);
                _interactableLocks.Push(UI.LockInteractable());
            }
        }

        protected virtual void ScreenDidNavigate(StackNavigationContext context)
        {
            if (context.NavigatingSourceType is StackNavigationSourceType.Insert or StackNavigationSourceType.Remove)
            {
                return;
            }

            if (context.ToScreen == this)
            {
                var stateEvent = context.NavigatingSourceType switch
                {
                    StackNavigationSourceType.Push => ScreenLifecycleEvent.ScreenDidStart,
                    StackNavigationSourceType.Pop => ScreenLifecycleEvent.ScreenDidResume,
                    _ => throw new ArgumentOutOfRangeException()
                };

                _interactableLocks.Pop().Dispose();
                ScreenEventInvoker.Invoke(stateEvent);
            }
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
            ScreenWillNavigate(stackContext);
        }

        void IScreenNavigatorEventHandler.ScreenDidNavigate(NavigationContext context)
        {
            var stackContext = context.ToStackNavigationContext();
            ScreenDidNavigate(stackContext);
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

        public void ForceUnlockInteractable()
        {
            while (_interactableLocks.Count > 0)
            {
                var lockObj = _interactableLocks.Pop();
                lockObj.Dispose();
            }
        }

        public virtual void Dispose()
        {
            Disposables.DisposeAll();
            Disposables.Clear();
        }

        public virtual async ValueTask DisposeAsync()
        {
            await UI.DisposeAsync();
            await AsyncDisposables.DisposeAllAsync();
            AsyncDisposables.Clear();
        }
    }
}