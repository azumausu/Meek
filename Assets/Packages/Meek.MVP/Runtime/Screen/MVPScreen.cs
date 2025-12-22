using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Meek.NavigationStack;
using Meek.UGUI;

namespace Meek.MVP
{
    public abstract class MVPScreen<TModel, TParam> : MVPScreen<TModel>
    {
        private TParam _parameter;

        protected abstract ValueTask<TModel> CreateModelAsync(TParam parameter);

        protected override ValueTask<TModel> CreateModelAsync()
        {
            return CreateModelAsync(_parameter);
        }

        protected override async ValueTask StartingImplAsync(StackNavigationContext context)
        {
            _parameter = context.GetNextScreenParameter<TParam>();
            await base.StartingImplAsync(context);
        }
    }

    public abstract class MVPScreen<TModel> : StackScreen
    {
        #region Fields

        public TModel Model { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// この関数を継承してモデルを作成してください
        /// </summary>
        protected abstract ValueTask<TModel> CreateModelAsync();

        /// <summary>
        /// この関数を継承してScreenのEventを登録してください
        /// </summary>
        protected abstract void RegisterEvents(EventHolder eventHolder, TModel model);

        protected override void RegisterEventsInternal(EventHolder eventHolder)
        {
            RegisterEvents(eventHolder, Model);
        }

        protected override async ValueTask StartingImplAsync(StackNavigationContext context)
        {
            Model = await CreateModelAsync();

            if (Model is IAsyncDisposable asyncDisposable) AsyncDisposables.Add(asyncDisposable);
            if (Model is IDisposable disposable) Disposables.Add(disposable);

            await base.StartingImplAsync(context);
        }

        public virtual async Task<TPresenter> LoadPresenterAsync<TPresenter>(
            IPrefabViewProvider viewProvider,
            [CanBeNull] object param = null
        )
            where TPresenter : class, IPresenter<TModel>
        {
            var presenterViewHandler = AppServices.GetService<IPresenterViewHandler>();
            await presenterViewHandler.InitializeAsync(viewProvider, this, param);
            await presenterViewHandler.LoadAsync(Model);

            UI.AddViewHandler(presenterViewHandler);

            return presenterViewHandler.GetPresenter<TPresenter>();
        }

        protected async Task<TPresenter> LoadPresenterAsync<TPresenter>([CanBeNull] object param = null)
            where TPresenter : class, IPresenter<TModel>
        {
            var provider = AppServices.GetService<IPresenterViewProvider>();
            provider.SetPrefabName(typeof(TPresenter).Name);
            return await LoadPresenterAsync<TPresenter>(provider, param);
        }

        #endregion
    }
}