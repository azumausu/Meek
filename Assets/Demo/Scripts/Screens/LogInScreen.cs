using System.Threading.Tasks;
using Meek.MVP;
using Meek.NavigationStack;
using UniRx;

namespace Demo
{
    public class LogInScreen : MVPScreen<LogInModel>
    {
        protected override async ValueTask<LogInModel> CreateModelAsync()
        {
            return await Task.FromResult(new LogInModel());
        }

        protected override void RegisterEvents(EventHolder eventHolder, LogInModel model)
        {
            eventHolder.ScreenWillStart(async () =>
            {
                var presenter = await LoadPresenterAsync<LogInPresenter>();

                presenter.OnClickBack.Subscribe(_ => PopNavigation.PopAsync().Forget());
                presenter.OnClickLogIn.Subscribe(_ => PushNavigation.PushAsync<HomeScreen>().Forget());

                presenter.OnEndEditEmail.Subscribe(model.UpdateEmail);
                presenter.OnEndEditPassword.Subscribe(model.UpdatePassword);
            });
        }
    }
}