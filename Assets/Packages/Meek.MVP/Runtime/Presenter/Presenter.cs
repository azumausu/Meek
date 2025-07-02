using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Meek.MVP
{
    public abstract class Presenter<TModel> : MonoBehaviour, IPresenter<TModel>, IAsyncDisposable
    {
        private readonly List<IDisposable> _disposables = new List<IDisposable>();
        private readonly List<IPresenterEventHandler> _presenterEventHandlers = new List<IPresenterEventHandler>();
        private TModel _model;

        private void Awake()
        {
            var handlers = this.GetComponents<IPresenterEventHandler>();
            if (handlers != null && handlers.Length > 0) _presenterEventHandlers.AddRange(handlers);

            OnInit();
            foreach (var handler in _presenterEventHandlers) handler.PresenterDidInit(this);
        }

        private void OnDestroy()
        {
            OnDeinit(_model);
            foreach (var handler in _presenterEventHandlers) handler.PresenterDidDeinit(this, _model);
            _disposables.DisposeAll();
        }

        private void Bind()
        {
            _disposables.AddRange(Bind(_model));
            foreach (var handler in _presenterEventHandlers) handler.PresenterDidBind(this, _model);
        }

        protected abstract IEnumerable<IDisposable> Bind(TModel model);

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
            foreach (var handler in _presenterEventHandlers) handler.PresenterDidSetup(this, _model);
            Bind();
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            await DisposeAsync();
        }
    }
}