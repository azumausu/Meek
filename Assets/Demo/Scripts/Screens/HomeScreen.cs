using System.Threading.Tasks;
using Meek.MVP;
using Meek.NavigationStack;
using Sample;
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
                presenter.OnClickShop.Subscribe(_ => model.UpdateTab(TabType.Shop));
                presenter.OnClickBag.Subscribe(_ => model.UpdateTab(TabType.Bag));
                presenter.OnClickFavorites.Subscribe(_ => model.UpdateTab(TabType.Favorites));
                presenter.OnClickProfile.Subscribe(_ => model.UpdateTab(TabType.Profile));
                presenter.OnClickProduct.Subscribe(x =>
                {
                    _stackNavigationService.PushAsync<SelectSizeScreen>(new SelectSizeScreenParameter()
                    {
                        ProductId = x,
                    }).Forget();
                });
            });
            
            eventHolder.SubscribeDispatchEvent<AddToCartEventArgs>(x =>
            {
                model.AddProduct(x.ProductId);
                return true;
            });
        }
    }
}