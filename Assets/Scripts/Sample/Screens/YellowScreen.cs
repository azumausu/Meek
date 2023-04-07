using System;
using System.Threading.Tasks;
using Meek.MVP;
using Meek.NavigationStack;
using MVP.Models;
using MVP.Presenters;
using UniRx;

namespace Sample
{
    public class YellowScreen : MVPScreen<YellowModel>
    {
        private readonly StackNavigationService _stackNavigationService;
        
        public YellowScreen(StackNavigationService stackNavigationService)
        {
            _stackNavigationService = stackNavigationService;
        }
        
        protected override async ValueTask<YellowModel> CreateModelAsync()
        {
            return await Task.FromResult(new YellowModel());
        }

        protected override void RegisterEvents(EventHolder eventHolder, YellowModel model)
        {
            ScreenLifecycleEventHolderExtension.ScreenWillStart(eventHolder, (Func<Task>)(async () =>
            {
                var presenter = await LoadPresenterAsync<YellowPresenter>();
                presenter.OnClickExit.Subscribe(_ => _stackNavigationService.PopAsync().Forget());
            }));
        }
    }
}