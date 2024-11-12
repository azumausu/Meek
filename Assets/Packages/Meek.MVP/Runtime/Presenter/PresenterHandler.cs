using System;
using System.Threading.Tasks;
using Meek.UGUI;
using UnityEngine;
using UnityEngine.UI;

namespace Meek.MVP
{
    public class PresenterHandler : PrefabViewHandler
    {
        private readonly IPresenter _presenter;

        private PresenterHandler(GameObject prefab) : base(prefab)
        {
            _presenter = Instance.GetComponent<IPresenter>();
        }

        protected override void Setup()
        {
            _presenter.Setup();
        }

        public static async ValueTask<PresenterHandler> CreateAsync<TModel>(GameObject prefab, TModel model)
        {
            var presenterHandler = new PresenterHandler(prefab);

            presenterHandler.SetInteractable(false);
            presenterHandler.SetVisibility(false);

            // Presenter初期化
            var presenter = presenterHandler.Instance.GetComponent<IPresenter<TModel>>();
            if (presenter == null)
            {
                Debug.LogError($"PresenterがRootNodeについていません。Instance名: {presenterHandler.Instance.name}");
                return null;
            }

            await presenter.LoadAsync(model);

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

        public override async ValueTask DisposeAsync()
        {
            await base.DisposeAsync();

            if (_presenter is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync();
            }
        }
    }
}