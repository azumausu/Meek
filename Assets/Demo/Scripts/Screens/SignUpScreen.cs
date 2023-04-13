using System.Threading.Tasks;
using Meek.MVP;
using Meek.NavigationStack;
using UniRx;

namespace Demo
{
    public class SignUpScreen : MVPScreen<SignUpModel>
    {
        protected override async ValueTask<SignUpModel> CreateModelAsync()
        {
            return await Task.FromResult(new SignUpModel());
        }

        protected override void RegisterEvents(EventHolder eventHolder, SignUpModel model)
        { 
            eventHolder.ScreenWillStart(async () =>
            {
                var presenter = await LoadPresenterAsync<SignUpPresenter>();

                presenter.OnClickBack.Subscribe(_ => PopNavigation.PopAsync().Forget());
                presenter.OnClickSignUp.Subscribe(_ => SignUpAsync().Forget());
                presenter.OnClickLogIn.Subscribe(_ =>
                {
                    PushNavigation.PushAsync<LogInScreen>().Forget();
                });

                presenter.OnEndEditName.Subscribe(model.UpdateName);
                presenter.OnEndEditEmail.Subscribe(model.UpdateEmail);
                presenter.OnEndEditPassword.Subscribe(model.UpdatePassword);
            });

            async Task SignUpAsync()
            {
                using var disposable = UI.LockInteractable();
                    
                await model.SignUpAsync();
                await PushNavigation.PushAsync<HomeScreen>(); 
            }
        }
    }
}