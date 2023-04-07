using System.Threading.Tasks;
using Meek.NavigationStack;
using UnityEngine;

namespace Meek.MVP
{
    public class PresenterLoaderFromResources<TModel> : IViewHandlerLoader
    {
        private readonly TModel _model;
        private readonly string _prefabPath;

        public bool IsLoaded { get; private set; } = false;
        
        public PresenterLoaderFromResources(TModel model, string prefabPath)
        {
            _model = model;
            _prefabPath = prefabPath;
        }
        
        async Task<IViewHandler> IViewHandlerLoader.LoadAsync()
        {
            // Prefabのロード
            var tcs = new TaskCompletionSource<GameObject>();
            var resourceRequest = Resources.LoadAsync<GameObject>(_prefabPath);
            resourceRequest.completed += _ => tcs.SetResult(resourceRequest.asset as GameObject);
            if (resourceRequest.isDone) tcs.SetResult(resourceRequest.asset as GameObject);
            var prefab = await tcs.Task;
            if (prefab == null)
            {
                Debug.LogError($"Prefabをロードできませんでした。Path: {_prefabPath}");
                return null;
            }
            
            // Viewの作成
            var viewHandler = await PresenterHandler.CreateAsync(prefab, _model);
            IsLoaded = true;
            return viewHandler;
        }
    }
}