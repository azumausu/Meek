using System.Threading.Tasks;
using Meek.MVP;
using Meek.NavigationStack;
using UniRx;
using UnityEngine;

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
                presenter.OnClickLogIn.Subscribe(_ => PushNavigation.PushAsync<TabScreen>().Forget());

                presenter.OnEndEditEmail.Subscribe(model.UpdateEmail);
                presenter.OnEndEditPassword.Subscribe(model.UpdatePassword);
            });

            eventHolder.ScreenDidStart(() => { Debug.Log($"Screen Did Start: {this.GetType().Name}"); });
        }
    }
}