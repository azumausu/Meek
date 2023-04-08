using System.Threading.Tasks;
using Meek.MVP;
using Meek.NavigationStack;
using Sample;
using UniRx;

namespace Demo
{
    public class LogInScreen : MVPScreen<LogInModel>
    {
        private readonly StackNavigationService _stackNavigationService;
        
        public LogInScreen(StackNavigationService stackNavigationService)
        {
            _stackNavigationService = stackNavigationService;
        }
        
        protected override async ValueTask<LogInModel> CreateModelAsync()
        {
            return await Task.FromResult(new LogInModel());
        }

        protected override void RegisterEvents(EventHolder eventHolder, LogInModel model)
        {
            eventHolder.ScreenWillStart(async () =>
            {
                var presenter = await LoadPresenterAsync<LogInPresenter>();

                presenter.OnClickBack.Subscribe(_ => _stackNavigationService.PopAsync().Forget());
                presenter.OnClickLogIn.Subscribe(_ => _stackNavigationService.PushAsync<HomeScreen>().Forget());
                presenter.OnClickSocialLoginAsFacebook.Subscribe(_ => _stackNavigationService.PushAsync<HomeScreen>().Forget());
                presenter.OnClickSocialLoginAsGoogle.Subscribe(_ => _stackNavigationService.PushAsync<HomeScreen>().Forget());

                presenter.OnEndEditEmail.Subscribe(model.UpdateEmail);
                presenter.OnEndEditPassword.Subscribe(model.UpdatePassword);
            });
        }
    }
}