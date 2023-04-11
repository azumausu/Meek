using System.Threading.Tasks;
using Meek.MVP;
using Meek.NavigationStack;
using UniRx;

namespace Demo
{
    public class HomeScreen : MVPScreen<HomeModel>
    {
        private readonly StackNavigationService _stackNavigationService;
        
        public HomeScreen(StackNavigationService stackNavigationService)
        {
            _stackNavigationService = stackNavigationService;
        }
        
        protected override async ValueTask<HomeModel> CreateModelAsync()
        {
            return await Task.FromResult(new HomeModel());
        }
        
        protected override void RegisterEvents(EventHolder eventHolder, HomeModel model)
        {
            eventHolder.ScreenWillStart(async () =>
            {
                var presenter = await LoadPresenterAsync<HomePresenter>();
                
                presenter.OnClickHome.Subscribe(_ => model.UpdateTab(TabType.Home));
                presenter.OnClickSearch.Subscribe(_ => model.UpdateTab(TabType.Search));
                presenter.OnClickFavorites.Subscribe(_ => model.UpdateTab(TabType.Favorites));
                presenter.OnClickProfile.Subscribe(_ => model.UpdateTab(TabType.Profile));
                presenter.OnClickProduct.Subscribe(index =>
                {
                    _stackNavigationService.PushAsync<ReviewScreen>(new ReviewScreenParameter()
                    {
                        ProductId = index + 1,
                    }).Forget();
                });
            });
            
            eventHolder.SubscribeDispatchEvent<ReviewEventArgs>(x =>
            {
                model.AddProduct(x.ProductId, x.IsGood);
                return true;
            });
        }
    }
}