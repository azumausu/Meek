using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Pool;

namespace Meek.MVP
{
    public abstract class Presenter<TModel> : MonoBehaviour, IPresenter<TModel>, IAsyncDisposable
    {
        [CanBeNull] private List<IDisposable> _disposables;
        [CanBeNull] private List<IPresenterEventHandler> _presenterEventHandlers;
        private TModel _model;

        private void Awake()
        {
            using var disposable = ListPool<IPresenterEventHandler>.Get(out var handlers);
            GetComponents(handlers);
            if (handlers.Count > 0)
            {
                if (_presenterEventHandlers == null)
                {
                    _presenterEventHandlers = new List<IPresenterEventHandler>(handlers);
                }
            }

            OnInit();

            if (_presenterEventHandlers != null)
            {
                foreach (var handler in _presenterEventHandlers)
                {
                    handler.PresenterDidInit(this);
                }
            }
        }

        private void OnDestroy()
        {
            try
            {
                if (_presenterEventHandlers != null)
                {
                    foreach (var handler in _presenterEventHandlers)
                    {
                        handler.PresenterDidDeinit(this, _model);
                    }
                }

                OnDeinit(_model);
            }
            finally
            {
                _disposables?.DisposeAll();
            }
        }

        private void Bind()
        {
            foreach (var disposable in Bind(_model))
            {
                if (_disposables == null)
                {
                    _disposables = new List<IDisposable>();
                }

                _disposables.Add(disposable);
            }

            if (_presenterEventHandlers != null)
            {
                foreach (var handler in _presenterEventHandlers)
                {
                    handler.PresenterDidBind(this, _model);
                }
            }
        }

        protected virtual IEnumerable<IDisposable> Bind(TModel model) { yield break; }

        protected virtual void OnInit() { }

        protected virtual Task LoadAsync(TModel model) { return Task.CompletedTask; }
        protected virtual Task DisposeAsync() { return Task.CompletedTask; }
        protected virtual void OnSetup(TModel model) { }
        protected virtual void OnDeinit(TModel model) { }

        Task IPresenter<TModel>.LoadAsync(TModel model)
        {
            _model = model;
            return LoadAsync(_model);
        }

        void IPresenter.Setup()
        {
            OnSetup(_model);

            if (_presenterEventHandlers != null)
            {
                foreach (var handler in _presenterEventHandlers)
                {
                    handler.PresenterDidSetup(this, _model);
                }
            }

            Bind();
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            await DisposeAsync();
        }
    }
}