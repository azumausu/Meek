using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Meek.NavigationStack
{
    public abstract class StackScreen : IScreen, IDisposable, IAsyncDisposable, IScreenLifecycleEventHandler, IScreenNavigatorEventHandler
    {
        private readonly Stack<IDisposable> _interactableLocks = new Stack<IDisposable>();
        private ICoroutineRunner _coroutineRunner;

        public readonly List<IDisposable> Disposables = new List<IDisposable>();
        public readonly List<IAsyncDisposable> AsyncDisposables = new List<IAsyncDisposable>();

        public IServiceProvider AppServices;
        public ScreenUI UI { get; private set; }
        public IScreenEventInvoker ScreenEventInvoker { get; private set; }
        public StackNavigationService NavigationService => AppServices.GetService<StackNavigationService>();

        protected virtual PushNavigation PushNavigation => AppServices.GetService<PushNavigation>().SetSender(this);
        protected virtual PopNavigation PopNavigation => AppServices.GetService<PopNavigation>().SetSender(this);
        protected virtual RemoveNavigation RemoveNavigation => AppServices.GetService<RemoveNavigation>().SetSender(this);
        protected virtual InsertNavigation InsertNavigation => AppServices.GetService<InsertNavigation>().SetSender(this);
        protected virtual BackToNavigation BackToNavigation => AppServices.GetService<BackToNavigation>().SetSender(this);

        protected virtual void Dispatch<TParam>(TParam param) => AppServices.GetService<StackNavigationService>().Dispatch(param);

        protected virtual async Task DispatchAsync<TParam>(TParam param) =>
            await AppServices.GetService<StackNavigationService>().DispatchAsync(param);

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

            // Unlocking is handled by ScreenDidNavigate.FinalizeIfPostActive.
            _interactableLocks.Push(UI.LockInteractable());

            ScreenEventInvoker.Invoke(ScreenLifecycleEvent.ScreenWillStart, context);
            await ScreenEventInvoker.InvokeAsync(ScreenLifecycleEvent.ScreenWillStart, context);

            UI.SetOpenAnimationStartTime(context);
            UI.Setup(context);
        }

        protected virtual async ValueTask ResumingImplAsync(StackNavigationContext context)
        {
            ScreenEventInvoker.Invoke(ScreenLifecycleEvent.ScreenWillResume, context);
            await ScreenEventInvoker.InvokeAsync(ScreenLifecycleEvent.ScreenWillResume, context);
        }

        protected virtual async ValueTask PausingImplAsync(StackNavigationContext context)
        {
            ScreenEventInvoker.Invoke(ScreenLifecycleEvent.ScreenDidPause, context);
            await ScreenEventInvoker.InvokeAsync(ScreenLifecycleEvent.ScreenDidPause, context);
        }

        protected virtual async ValueTask DestroyingImplAsync(StackNavigationContext context)
        {
            ScreenEventInvoker.Invoke(ScreenLifecycleEvent.ScreenDidDestroy, context);
            await ScreenEventInvoker.InvokeAsync(ScreenLifecycleEvent.ScreenDidDestroy, context);

            if (AutoDisposeLockerOnDestroy)
            {
                ForceUnlockInteractable();
            }

            _interactableLocks?.Clear();
        }

        protected virtual void ScreenWillNavigate(StackNavigationContext context)
        {
            switch (context.NavigatingSourceType)
            {
                case StackNavigationSourceType.Push:
                    Push();
                    break;
                case StackNavigationSourceType.Pop:
                    Pop();
                    break;
                case StackNavigationSourceType.Insert:
                    Insert();
                    break;
                case StackNavigationSourceType.Remove:
                    Remove();
                    break;
            }

            return;

            void Push()
            {
                // Prepare If PreActive
                if (context.FromScreen == this)
                {
                    ScreenEventInvoker.Invoke(ScreenLifecycleEvent.ScreenWillPause, context);
                    _interactableLocks.Push(UI.LockInteractable());
                    // Unlock interactable if error occurs during navigation
                    context.OnError += _ =>
                    {
                        if (_interactableLocks.TryPop(out var interactableLock))
                        {
                            interactableLock.Dispose();
                        }
                    };
                }
            }

            void Pop()
            {
                // Prepare If PreActive
                if (context.FromScreen == this)
                {
                    ScreenEventInvoker.Invoke(ScreenLifecycleEvent.ScreenWillDestroy, context);
                }
            }

            void Insert()
            {
            }

            void Remove()
            {
            }
        }

        protected virtual void ScreenDidNavigate(StackNavigationContext context)
        {
            switch (context.NavigatingSourceType)
            {
                case StackNavigationSourceType.Push:
                    Push();
                    break;
                case StackNavigationSourceType.Pop:
                    Pop();
                    break;
                case StackNavigationSourceType.Insert:
                    Insert();
                    break;
                case StackNavigationSourceType.Remove:
                    Remove();
                    break;
            }

            return;

            void Push()
            {
                // Finalize If PostActive
                if (context.ToScreen == this)
                {
                    // The lock released at this point is the lock that was added in StartingImplAsync().
                    if (_interactableLocks.TryPop(out var interactableLock))
                    {
                        interactableLock.Dispose();
                    }

                    ScreenEventInvoker?.Invoke(ScreenLifecycleEvent.ScreenDidStart, context);
                }
            }

            void Pop()
            {
                // Finalize If PostActive
                if (context.ToScreen == this)
                {
                    //  The lock released at this point is the lock that was added in ScreenWillNavigate.Push().
                    if (_interactableLocks.TryPop(out var interactableLock))
                    {
                        interactableLock.Dispose();
                    }

                    ScreenEventInvoker?.Invoke(ScreenLifecycleEvent.ScreenDidResume, context);
                }
            }

            void Insert()
            {
                if (context.GetInsertionScreen() == this)
                {
                    ScreenEventInvoker?.Invoke(ScreenLifecycleEvent.ScreenDidStart, context);
                }
            }

            void Remove()
            {
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

        ValueTask IScreenLifecycleEventHandler.StartingImplAsync(StackNavigationContext context)
        {
            return StartingImplAsync(context);
        }

        ValueTask IScreenLifecycleEventHandler.ResumingImplAsync(StackNavigationContext context)
        {
            return ResumingImplAsync(context);
        }

        ValueTask IScreenLifecycleEventHandler.PausingImplAsync(StackNavigationContext context)
        {
            return PausingImplAsync(context);
        }

        ValueTask IScreenLifecycleEventHandler.DestroyingImplAsync(StackNavigationContext context)
        {
            return DestroyingImplAsync(context);
        }

        void IScreenNavigatorEventHandler.ScreenWillNavigate(StackNavigationContext context)
        {
            ScreenWillNavigate(context);
        }

        void IScreenNavigatorEventHandler.ScreenDidNavigate(StackNavigationContext context)
        {
            ScreenDidNavigate(context);
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