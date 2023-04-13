using System.Threading.Tasks;
using Demo.ApplicationServices;
using Meek.MVP;
using Meek.NavigationStack;
using UniRx;

namespace Demo
{
    public class HomeScreen : MVPScreen<HomeModel>
    {
        private readonly GlobalStore _globalStore;
        
        public HomeScreen(GlobalStore globalStore)
        {
            _globalStore = globalStore;
        }
        
        protected override async ValueTask<HomeModel> CreateModelAsync()
        {
            return await Task.FromResult(new HomeModel(_globalStore));
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
                   PushNavigation.UpdateNextScreenParameter(new ReviewScreenParameter(){ ProductId = index + 1, })
                       .PushAsync<ReviewScreen>().Forget();
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