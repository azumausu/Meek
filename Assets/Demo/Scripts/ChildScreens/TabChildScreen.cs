using UniRx;
using Meek.MVP;
using Meek.NavigationStack;
using Meek.NavigationStack.Child;
using Meek.UGUI;

namespace Sample
{
    public class TabChildScreen : ChildStackScreen<TabModel>
    {
        private readonly ChildStackNavigationService _childStackNavigationService;
        private readonly IPrefabViewManager _prefabViewManager;

        public TabChildScreen(
            ChildStackNavigationService childStackNavigationService,
            IPrefabViewManager prefabViewManager)
        {
            _childStackNavigationService = childStackNavigationService;
            _prefabViewManager = prefabViewManager;
        }

        protected override TabModel CreateModel(ChildStackNavigationContext context)
        {
            return new TabModel();
        }

        protected override IViewHandlerLoader CreateLoader()
        {
            var presenterLoader = new PresenterLoaderFactoryFromResources();
            return presenterLoader.CreateLoader(Model, "TabPresenter");
        }

        protected override void Bind(IViewHandler viewHandler)
        {
            if (viewHandler is PrefabViewHandler prefabViewHandler)
                _prefabViewManager.AddInHierarchy(prefabViewHandler);

            var PrefabViewHandler = viewHandler as PrefabViewHandler;
            var presenter = PrefabViewHandler.Instance.GetComponent<TabPresenter>();
            presenter.OnClick.Subscribe(_ => _childStackNavigationService.PopAsync(ParentScreen).Forget());
        }
    }
}