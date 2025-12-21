using System.Threading.Tasks;
using Meek.NavigationStack;
using Meek.UGUI;
using UnityEngine;
using UnityEngine.UI;

namespace Meek.MVP
{
    public class DynamicPresenterViewHandler : DynamicPrefabViewHandler, IPresenterViewHandler
    {
        public DynamicPresenterViewHandler(IPrefabViewManager prefabViewManager) : base(prefabViewManager)
        {
        }

        public async ValueTask LoadAsync<TModel>(TModel model)
        {
            // Presenter初期化
            var presenter = Instance.GetComponent<IPresenter<TModel>>();
            if (presenter == null)
            {
                Debug.LogError($"Presenter is not attached to the RootNode. Instance Name: {Instance.name}");
                return;
            }

            await presenter.LoadAsync(model);

            var graphicRaycasters = Instance.GetComponentsInChildren<GraphicRaycaster>(true);
            var visibilitySwitchers = Instance.GetComponentsInChildren<IVisibilitySwitcher>(true);
            var inputSwitchers = Instance.GetComponentsInChildren<IInputSwitcher>(true);

            // 新たに取得できたものをfalse状態にする
            foreach (var switcher in inputSwitchers) switcher.Disable();
            foreach (var switcher in visibilitySwitchers) switcher.Hide();

            foreach (var graphicRaycaster in graphicRaycasters)
            {
                GraphicRaycasters.Add(graphicRaycaster);
            }

            foreach (var visibilitySwitcher in visibilitySwitchers)
            {
                VisibilitySwitchers.Add(visibilitySwitcher);
            }

            foreach (var inputSwitcher in inputSwitchers)
            {
                InputSwitchers.Add(inputSwitcher);
            }
        }

        public TPresenter GetPresenter<TPresenter>() where TPresenter : IPresenter
        {
            var presenter = Instance.GetComponent<TPresenter>();
            if (presenter == null)
            {
                Debug.LogError(
                    $"Presenter of type {typeof(TPresenter).Name} is not attached to the RootNode. Instance Name: {Instance.name}");
            }

            return presenter;
        }

        protected override void Setup(StackNavigationContext context)
        {
            var presenter = Instance.GetComponent<IPresenter>();
            if (presenter != null)
            {
                presenter.Setup();
            }
        }
    }
}