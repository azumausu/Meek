using System.Threading.Tasks;
using Meek.MVP;
using Meek.NavigationStack;
using UniRx;

namespace Demo
{
    public class SplashScreen : MVPScreen<SplashModel>
    {
        private readonly StackNavigationService _stackNavigationService;
        
        public SplashScreen(StackNavigationService stackNavigationService)
        {
            _stackNavigationService = stackNavigationService;
        }
        
        protected override async ValueTask<SplashModel> CreateModelAsync()
        {
            return await Task.FromResult(new SplashModel());
        }

        protected override void RegisterEvents(EventHolder eventHolder, SplashModel model)
        {
            eventHolder.ScreenWillStart(async () =>
            {
                var presenter = await LoadPresenterAsync<SplashPresenter>();

                presenter.OnClickSignUp.Subscribe(_ => _stackNavigationService.PushAsync<SignUpScreen>());
                presenter.OnClickLogIn.Subscribe(_ => _stackNavigationService.PushAsync<LogInScreen>());
            });
        }
    }
}