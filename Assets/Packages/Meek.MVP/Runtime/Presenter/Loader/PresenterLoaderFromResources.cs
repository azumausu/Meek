using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Meek.NavigationStack;
using Meek.UGUI;
using UnityEngine;
using UnityEngine.UI;

namespace Meek.MVP
{
    public class PresenterLoaderFromResources<TModel> : IViewHandlerLoader
    {
        private readonly IScreen _ownerScreen;
        private readonly TModel _model;
        private readonly IPrefabViewManager _prefabViewManager;
        private readonly string _prefabPath;

        public bool IsLoaded { get; private set; } = false;

        public PresenterLoaderFromResources(IScreen ownerScreen, TModel model, IPrefabViewManager prefabViewManager, string prefabPath)
        {
            _ownerScreen = ownerScreen;
            _model = model;
            _prefabViewManager = prefabViewManager;
            _prefabPath = prefabPath;
        }

        async Task<IViewHandler> IViewHandlerLoader.LoadAsync([CanBeNull] object param)
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
            var viewHandler = await CreateAsync(prefab, param);
            IsLoaded = true;
            return viewHandler;
        }

        private async ValueTask<PresenterHandler> CreateAsync(GameObject prefab, [CanBeNull] object param)
        {
            var parent = _prefabViewManager.GetRootNode(_ownerScreen, param);
            var rootNode = new GameObject(prefab.name) { layer = parent.gameObject.layer, transform = { parent = parent } };
            var rootNodeRectTransform = rootNode.AddComponent<RectTransform>();
            rootNodeRectTransform.anchoredPosition3D = Vector3.zero;
            rootNodeRectTransform.anchorMin = Vector2.zero;
            rootNodeRectTransform.anchorMax = Vector2.one;
            rootNodeRectTransform.sizeDelta = Vector2.zero;
            rootNodeRectTransform.localScale = Vector3.one;

            var rootNodeCanvas = rootNode.AddComponent<Canvas>();
            rootNodeCanvas.overrideSorting = false;
            rootNode.AddComponent<CanvasGroup>();
            rootNode.AddComponent<GraphicRaycaster>();

            var instance = GameObject.Instantiate(prefab, rootNode.transform);

            var presenterHandler = new PresenterHandler(rootNode.transform, instance);
            presenterHandler.SetInteractable(false);
            presenterHandler.SetVisibility(false);

            // Presenter初期化
            var presenter = presenterHandler.Instance.GetComponent<IPresenter<TModel>>();
            if (presenter == null)
            {
                Debug.LogError($"PresenterがRootNodeについていません。Instance名: {presenterHandler.Instance.name}");
                return null;
            }

            await presenter.LoadAsync(_model);

            var graphicRaycasters = presenterHandler.Instance.GetComponentsInChildren<GraphicRaycaster>(true);
            var visibilitySwitchers = presenterHandler.Instance.GetComponentsInChildren<IVisibilitySwitcher>(true);
            var inputSwitchers = presenterHandler.Instance.GetComponentsInChildren<IInputSwitcher>(true);

            // 新たに取得できたものをfalse状態にする
            foreach (var switcher in inputSwitchers) switcher.Disable();
            foreach (var switcher in visibilitySwitchers) switcher.Hide();

            foreach (var graphicRaycaster in graphicRaycasters)
                presenterHandler.GraphicRaycasters.Add(graphicRaycaster);
            foreach (var visibilitySwitcher in visibilitySwitchers)
                presenterHandler.VisibilitySwitchers.Add(visibilitySwitcher);
            foreach (var inputSwitcher in inputSwitchers)
                presenterHandler.InputSwitchers.Add(inputSwitcher);

            return presenterHandler;
        }
    }
}