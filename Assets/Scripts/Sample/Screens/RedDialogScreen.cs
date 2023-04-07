using System;
using System.Threading.Tasks;
using Meek.MVP;
using Meek.NavigationStack;
using MVP.Models;
using MVP.Presenters;
using UniRx;

namespace Sample
{
    public class RedDialogScreen : MVPScreen<RedDialogModel>
    {
        private readonly StackNavigationService _stackNavigationService;
        
        public RedDialogScreen(StackNavigationService stackNavigationService)
        {
            _stackNavigationService = stackNavigationService;
        }
        
        public override ScreenUIType ScreenUIType => ScreenUIType.WindowOrTransparent;
        
        protected override async ValueTask<RedDialogModel> CreateModelAsync()
        {
            return await Task.FromResult(new RedDialogModel());
        }

        protected override void RegisterEvents(EventHolder eventHolder, RedDialogModel model)
        {
            ScreenLifecycleEventHolderExtension.ScreenWillStart(eventHolder, (Func<Task>)(async () =>
            {
                var presenter = await LoadPresenterAsync<RedDialogPresenter>();
                presenter.OnClickExit.Subscribe(_ => _stackNavigationService.PopAsync().Forget());
            }));
        }
    }
}