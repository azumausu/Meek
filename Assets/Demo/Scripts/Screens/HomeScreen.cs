using System.Threading.Tasks;
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
                presenter.OnClickProduct.Subscribe(id =>
                {
                   PushNavigation.UpdateNextScreenParameter(new ReviewScreenParameter(){ ProductId = id, })
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