using UniRx;
using System.Threading.Tasks;
using Meek.MVP;
using Meek.NavigationStack;
using Sample;

namespace Demo
{
    public class SelectSizeScreen : MVPScreen<SelectSizeModel>
    {
        private readonly StackNavigationService _stackNavigationService;
        
        public SelectSizeScreen(StackNavigationService stackNavigationService)
        {
            _stackNavigationService = stackNavigationService;
        }

        protected override async ValueTask<SelectSizeModel> CreateModelAsync()
        {
            return await Task.FromResult(new SelectSizeModel());
        }

        protected override void RegisterEvents(EventHolder eventHolder, SelectSizeModel model)
        {
            eventHolder.ScreenWillStart(async () =>
            {
                var presenter = await LoadPresenterAsync<SelectSizePresenter>();
                
                presenter.OnClickBack.Subscribe(_ => _stackNavigationService.PopAsync().Forget());

                presenter.OnClickXS.Subscribe(_ => model.UpdateSize(Size.XS));
                presenter.OnClickS.Subscribe(_ => model.UpdateSize(Size.S));
                presenter.OnClickM.Subscribe(_ => model.UpdateSize(Size.M));
                presenter.OnClickL.Subscribe(_ => model.UpdateSize(Size.L));
                presenter.OnClickXL.Subscribe(_ => model.UpdateSize(Size.XL));
            });
        }

        public override ScreenUIType ScreenUIType => ScreenUIType.WindowOrTransparent;
    }
}