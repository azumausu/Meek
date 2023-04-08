using UniRx;
using System.Threading.Tasks;
using Meek.MVP;
using Meek.NavigationStack;
using Sample;

namespace Demo
{
    public class SelectSizeScreen : MVPScreen<SelectSizeModel, SelectSizeScreenParameter>
    {
        private readonly StackNavigationService _stackNavigationService;

        public SelectSizeScreen(StackNavigationService stackNavigationService)
        {
            _stackNavigationService = stackNavigationService;
        }

        protected override async ValueTask<SelectSizeModel> CreateModelAsync(SelectSizeScreenParameter parameter)
        {
            return await Task.FromResult(new SelectSizeModel(parameter.ProductId));
        }

        protected override void RegisterEvents(EventHolder eventHolder, SelectSizeModel model)
        {
            eventHolder.ScreenWillStart(async () =>
            {
                var presenter = await LoadPresenterAsync<SelectSizePresenter>();

                presenter.OnClickBack.Subscribe(_ => _stackNavigationService.PopAsync().Forget());
                presenter.OnClickAddToCart.Subscribe(_ => AddToCartAsync().Forget());

                presenter.OnClickXS.Subscribe(_ => model.UpdateSize(SizeType.XS));
                presenter.OnClickS.Subscribe(_ => model.UpdateSize(SizeType.S));
                presenter.OnClickM.Subscribe(_ => model.UpdateSize(SizeType.M));
                presenter.OnClickL.Subscribe(_ => model.UpdateSize(SizeType.L));
                presenter.OnClickXL.Subscribe(_ => model.UpdateSize(SizeType.XL));
            });

            async Task AddToCartAsync()
            {
                await _stackNavigationService.PopAsync();
                _stackNavigationService.Dispatch(new AddToCartEventArgs());
            }
        }

        public override ScreenUIType ScreenUIType => ScreenUIType.WindowOrTransparent;
    }
}