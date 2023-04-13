using System;
using System.Reflection;
using System.Threading.Tasks;
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
            _parameter = context.GetFeatureValue<TParam>(StackNavigationContextFeatureDefine.NextScreenParameter);
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
            if (Model is IDisposable disposable) Disposables.Add(disposable);
            
            await base.StartingImplAsync(context);
        }

        protected async Task<TPresenter> LoadPresenterAsync<TPresenter>(string prefabName)
            where TPresenter : class, IPresenter<TModel>
        {
            var factory = AppServices.GetService<IPresenterLoaderFactory>();
            var loader = factory.CreateLoader(Model, prefabName);
            var viewHandler =  await UI.LoadViewHandlerAsync(loader) as PrefabViewHandler;
            var prefabViewManager = AppServices.GetService<IPrefabViewManager>();
            prefabViewManager.AddInHierarchy(viewHandler);
            
            return viewHandler.Instance.GetComponent<TPresenter>();
        }
        
        protected Task<TPresenter> LoadPresenterAsync<TPresenter>()
            where TPresenter : class, IPresenter<TModel>
        {
            return LoadPresenterAsync<TPresenter>(typeof(TPresenter).Name);
        }
        
        protected PushNavigation PushNavigation => AppServices.GetService<PushNavigation>();
        protected PopNavigation PopNavigation => AppServices.GetService<PopNavigation>();
        protected RemoveNavigation RemoveNavigation => AppServices.GetService<RemoveNavigation>();
        protected InsertNavigation InsertNavigation => AppServices.GetService<InsertNavigation>();
        protected BackToNavigation BackToNavigation => AppServices.GetService<BackToNavigation>();
        protected void Dispatch<TParam>(TParam param) => AppServices.GetService<StackNavigationService>().Dispatch(param);
        
        #endregion
    }
}