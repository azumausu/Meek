﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.Pool;

namespace Meek.NavigationStack
{
    public class ScreenUI : IDisposable
    {
        #region Fields

        private readonly CoroutineRunner _coroutineRunner;
        private readonly List<IDisposable> _disposables = new List<IDisposable>();
        private readonly List<IViewHandlerLoader> _viewHandlerLoaders = new List<IViewHandlerLoader>();
        private readonly List<IViewHandler> _viewHandlers = new List<IViewHandler>();
        private readonly LockObject _interactableLocker;
        
        public bool IsInteractable => !_interactableLocker.IsLock;
        public bool IsVisible { get; private set; }
        public bool IsLoaded => _viewHandlerLoaders.All(x => x.IsLoaded);
        public IReadOnlyCollection<IViewHandler> ViewHandlers => _viewHandlers;

        #endregion

        #region Constructors

        public ScreenUI(CoroutineRunner coroutineRunner)
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
        
        public async Task<IViewHandler> LoadViewHandlerAsync(IViewHandlerLoader viewHandlerLoader)
        {
            _viewHandlerLoaders.Add(viewHandlerLoader);
            var viewHandler = await viewHandlerLoader.LoadAsync();
            _disposables.Add(viewHandler);
            _viewHandlers.Add(viewHandler);
            
            return viewHandler;
        }

        public void DisposeViewHandler(IViewHandler viewHandler)
        {
            if (!_disposables.Remove(viewHandler)) return;
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

        internal void SetLayer(NavigationContext context)
        {
            // もしここで呼び出す場合も不都合あるならSetupCoroutine()のSetLayer版を作成する
            // Layerの設定はSetup前に行う
            foreach (var viewHandler in _viewHandlers) viewHandler.SetLayer();
            
            foreach (var viewController in _viewHandlers) 
                viewController.EvaluateNavigateAnimation(
                    NavigatorAnimationType.Open,
                    context.FromScreen?.GetType(),
                    context.ToScreen.GetType(),
                    0.0f
                );
        }

        internal void Setup(NavigationContext context)
        {
            foreach (var viewHandler in _viewHandlers) viewHandler.Setup();
        }

        internal IEnumerator OpenRoutine(Type fromScreenClassType, Type toScreenClassType, bool isImmediate = false)
        {
            return PlayNavigateAnimationRoutine(
                NavigatorAnimationType.Open, 
                fromScreenClassType, 
                toScreenClassType,
                isImmediate
            );
        }

        internal IEnumerator CloseRoutine(Type fromScreenClassType, Type toScreenClassType, bool isImmediate = false)
        {
            return PlayNavigateAnimationRoutine(
                NavigatorAnimationType.Close, 
                fromScreenClassType, 
                toScreenClassType,
                isImmediate
            );
        }

        internal IEnumerator ShowRoutine(Type fromScreenClassType, Type toScreenClassType, bool isImmediate = false)
        {
            return PlayNavigateAnimationRoutine(
                NavigatorAnimationType.Show, 
                fromScreenClassType, 
                toScreenClassType,
                isImmediate
            );
        }

        internal IEnumerator HideRoutine(Type fromScreenClassType, Type toScreenClassType, bool isImmediate = false)
        {
            return PlayNavigateAnimationRoutine(
                NavigatorAnimationType.Hide, 
                fromScreenClassType, 
                toScreenClassType,
                isImmediate
                );
        }

        private IEnumerator PlayNavigateAnimationRoutine(
            NavigatorAnimationType navigatorAnimationType,
            Type fromScreenClassType,
            Type toScreenClassType,
            bool isImmediate)
        {
            if (isImmediate)
            {
                foreach (var viewController in _viewHandlers) 
                    viewController.EvaluateNavigateAnimation(
                        navigatorAnimationType,
                        fromScreenClassType,
                        toScreenClassType,
                        1.0f
                    );

                yield break;
            }
            
            foreach (var viewController in _viewHandlers) 
                viewController.EvaluateNavigateAnimation(
                    navigatorAnimationType,
                    fromScreenClassType,
                    toScreenClassType,
                    0.0f
                );

            var coroutines = ListPool<IEnumerator>.Get();
            
            coroutines.AddRange(_viewHandlers
                .Select(x => x.PlayNavigateAnimationRoutine(
                    navigatorAnimationType, 
                    fromScreenClassType, 
                    toScreenClassType)
                )
            );
            yield return _coroutineRunner.StartParallelCoroutine(coroutines);
            
            ListPool<IEnumerator>.Release(coroutines); 
        }

        #endregion

        #region Interface Implementations

        public void Dispose()
        {
            foreach (var d in _disposables) d.Dispose();
        }

        #endregion
    }
}