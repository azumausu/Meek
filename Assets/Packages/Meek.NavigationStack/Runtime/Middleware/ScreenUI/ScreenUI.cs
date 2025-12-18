using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Pool;

namespace Meek.NavigationStack
{
    public class ScreenUI
    {
        #region Fields

        private readonly ICoroutineRunner _coroutineRunner;
        private readonly List<IViewHandlerLoader> _viewHandlerLoaders = new List<IViewHandlerLoader>();
        private readonly List<IViewHandler> _viewHandlers = new List<IViewHandler>();
        private readonly LockObject _interactableLocker;

        public bool IsInteractable => !_interactableLocker.IsLock;
        public bool IsVisible { get; private set; }
        public bool IsLoaded => _viewHandlerLoaders.All(x => x.IsLoaded);
        public IReadOnlyCollection<IViewHandler> ViewHandlers => _viewHandlers;

        #endregion

        #region Constructors

        public ScreenUI(ICoroutineRunner coroutineRunner)
        {
            _coroutineRunner = coroutineRunner;
            _interactableLocker = new LockObject(
                () =>
                {
                    foreach (var instance in _viewHandlers) instance.SetInteractable(false);
                },
                () =>
                {
                    foreach (var instance in _viewHandlers) instance.SetInteractable(true);
                }
            );
        }

        #endregion

        #region Methods

        public async Task<IViewHandler> LoadViewHandlerAsync(IViewHandlerLoader viewHandlerLoader, [CanBeNull] object param = null)
        {
            _viewHandlerLoaders.Add(viewHandlerLoader);
            try
            {
                var viewHandler = await viewHandlerLoader.LoadAsync(param);
                _viewHandlers.Add(viewHandler);

                return viewHandler;
            }
            catch
            {
                _viewHandlerLoaders.Remove(viewHandlerLoader);
                throw;
            }
        }

        public void DisposeViewHandler(IViewHandler viewHandler)
        {
            if (!_viewHandlers.Remove(viewHandler)) return;

            viewHandler.Dispose();
        }

        /// <summary>
        ///     UIのInteractableをLockします。
        /// </summary>
        public IDisposable LockInteractable()
        {
            return _interactableLocker.Lock();
        }

        /// <summary>
        ///     UIの表示非表示を切り替えます。
        /// </summary>
        public void SetVisible(bool visible)
        {
            IsVisible = visible;
            foreach (var instance in _viewHandlers) instance.SetVisibility(visible);
        }

        internal void SetOpenAnimationStartTime(StackNavigationContext context)
        {
            foreach (var viewController in _viewHandlers)
            {
                viewController.EvaluateNavigateAnimation(context, NavigatorAnimationType.Open, 0.0f);
            }
        }

        internal void Setup(StackNavigationContext context)
        {
            foreach (var viewHandler in _viewHandlers)
            {
                viewHandler.Setup(context);
            }
        }

        internal IEnumerator OpenRoutine(StackNavigationContext context, bool isImmediate = false)
        {
            return PlayNavigateAnimationRoutine(context, NavigatorAnimationType.Open, isImmediate);
        }

        internal IEnumerator CloseRoutine(StackNavigationContext context, bool isImmediate = false)
        {
            return PlayNavigateAnimationRoutine(context, NavigatorAnimationType.Close, isImmediate);
        }

        internal IEnumerator ShowRoutine(StackNavigationContext context, bool isImmediate = false)
        {
            return PlayNavigateAnimationRoutine(context, NavigatorAnimationType.Show, isImmediate);
        }

        internal IEnumerator HideRoutine(StackNavigationContext context, bool isImmediate = false)
        {
            return PlayNavigateAnimationRoutine(context, NavigatorAnimationType.Hide, isImmediate);
        }

        private IEnumerator PlayNavigateAnimationRoutine(
            StackNavigationContext context,
            NavigatorAnimationType navigatorAnimationType,
            bool isImmediate
        )
        {
            if (isImmediate)
            {
                foreach (var viewController in _viewHandlers)
                {
                    viewController.EvaluateNavigateAnimation(context, navigatorAnimationType, 1.0f);
                }

                yield break;
            }

            foreach (var viewController in _viewHandlers)
            {
                viewController.EvaluateNavigateAnimation(context, navigatorAnimationType, 0.0f);
            }

            using var disposable = ListPool<IEnumerator>.Get(out var coroutines);

            foreach (var handler in _viewHandlers)
            {
                coroutines.Add(handler.PlayNavigateAnimationRoutine(context, navigatorAnimationType));
            }

            yield return _coroutineRunner.StartParallelCoroutine(coroutines);
        }

        #endregion

        public async ValueTask DisposeAsync()
        {
            await _viewHandlers.DisposeAllAsync();
            _viewHandlers.DisposeAll();
            _viewHandlers.Clear();
        }
    }
}