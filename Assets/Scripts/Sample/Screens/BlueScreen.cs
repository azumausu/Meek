using System.Threading.Tasks;
using Harmos.UI.UGUI;
using Meek.MVP;
using Meek.NavigationStack;
using Meek.NavigationStack.Child;
using MVP.Models;
using MVP.Presenters;
using UniRx;

namespace Sample
{
    public class BlueScreen : MVPScreen<BlueModel>
    {
        private readonly StackNavigationService _stackNavigationService;
        private readonly ChildStackNavigationService _childStackNavigationService;

        public BlueScreen(
            StackNavigationService stackNavigationService,
            ChildStackNavigationService childStackNavigationService
        )
        {
            _stackNavigationService = stackNavigationService;
            _childStackNavigationService = childStackNavigationService;
        }

        protected override async ValueTask<BlueModel> CreateModelAsync()
        {
            return await Task.FromResult(new BlueModel());
        }

        protected override void RegisterEvents(EventHolder eventHolder, BlueModel model)
        {
            eventHolder.ScreenWillStart(
                async () =>
                {
                    var presenter = await LoadPresenterAsync<BluePresenter>();
                    presenter.OnClick.Subscribe(_ => _stackNavigationService.PushAsync<RedDialogScreen>().Forget());
                    presenter.OnClickExit.Subscribe(_ => _childStackNavigationService.PushAsync<TabChildScreen>(this).Forget());
                });
        }
    }
}