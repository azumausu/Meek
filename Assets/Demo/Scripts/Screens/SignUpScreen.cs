using System.Threading.Tasks;
using Meek.MVP;
using Meek.NavigationStack;
using Sample;
using UniRx;

namespace Demo
{
    public class SignUpScreen : MVPScreen<SignUpModel>
    {
        private readonly StackNavigationService _stackNavigationService;
        
        public SignUpScreen(StackNavigationService stackNavigationService)
        {
            _stackNavigationService = stackNavigationService;
        }
        
        protected override async ValueTask<SignUpModel> CreateModelAsync()
        {
            return await Task.FromResult(new SignUpModel());
        }

        protected override void RegisterEvents(EventHolder eventHolder, SignUpModel model)
        { 
            eventHolder.ScreenWillStart(async () =>
            {
                var presenter = await LoadPresenterAsync<SignUpPresenter>();

                presenter.OnClickSignUp.Subscribe(_ => SignUpAsync().Forget());
                presenter.OnClickAlreadyHaveAnAccount.Subscribe(_ =>
                {
                    _stackNavigationService.PushAsync<LogInScreen>().Forget();
                });

                presenter.OnEndEditName.Subscribe(model.UpdateName);
                presenter.OnEndEditEmail.Subscribe(model.UpdateEmail);
                presenter.OnEndEditPassword.Subscribe(model.UpdatePassword);
            });

            async Task SignUpAsync()
            {
                using var disposable = UI.LockInteractable();
                    
                await model.SignUpAsync();
                await _stackNavigationService.PushAsync<HomeScreen>(); 
            }
        }
    }
}