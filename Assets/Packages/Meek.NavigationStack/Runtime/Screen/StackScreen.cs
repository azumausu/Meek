using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Meek.NavigationStack
{
    public abstract class StackScreen : IScreen, IDisposable, IScreenLifecycleEventHandler, IScreenNavigatorEventHandler
    {
        private readonly Stack<IDisposable> _interactableLocks = new Stack<IDisposable>();

        protected IServiceProvider AppServices;
        protected List<IDisposable> Disposables = new List<IDisposable>();

        private ICoroutineRunner _coroutineRunner;

        public ScreenUI UI { get; private set; }
        public IScreenEventInvoker ScreenEventInvoker { get; private set; }
        
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
            ScreenEventInvoker.Invoke(ScreenLifecycleEvent.ScreenDidPause);
            await ScreenEventInvoker.InvokeAsync(ScreenLifecycleEvent.ScreenDidPause);
        }

        protected virtual async ValueTask PausingImplAsync(StackNavigationContext context)
        {
            ScreenEventInvoker.Invoke(ScreenLifecycleEvent.ScreenWillResume);
            await ScreenEventInvoker.InvokeAsync(ScreenLifecycleEvent.ScreenWillResume);
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

        public void Dispose()
        {
            UI.Dispose();
            Disposables.DisposeAll();
            Disposables.Clear(); 
        }
    }
}