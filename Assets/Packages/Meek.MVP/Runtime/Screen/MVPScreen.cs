using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Meek.NavigationStack;
using Meek.UGUI;

namespace Meek.MVP
{
    /// <summary>
    /// Presenterをロードする機能を追加したOsmiumScreen
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <typeparam name="TParam"></typeparam>
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
            // Parameterの代入を行う
            _parameter = context.GetNextScreenParameter<TParam>();
            // Parameter代入後にAppBaseScreen<TModel>のStartImplを呼び出す。
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

            // Disposableを実装している場合は登録しておく。
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