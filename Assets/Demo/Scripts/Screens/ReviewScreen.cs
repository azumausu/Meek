using System.Threading.Tasks;
using Meek.MVP;
using Meek.NavigationStack;
using UniRx;

namespace Demo
{
    public class ReviewScreen : MVPScreen<ReviewModel, ReviewScreenParameter>
    {
        private readonly StackNavigationService _stackNavigationService;
        
        public ReviewScreen(StackNavigationService stackNavigationService)
        {
            _stackNavigationService = stackNavigationService;
        }
        
        protected override async ValueTask<ReviewModel> CreateModelAsync(ReviewScreenParameter parameter)
        {
            return await Task.FromResult(new ReviewModel(parameter.ProductId));
        }

        protected override void RegisterEvents(EventHolder eventHolder, ReviewModel model)
        {
            eventHolder.ScreenWillStart(async () =>
            {
                var presenter = await LoadPresenterAsync<ReviewPresenter>();

                presenter.OnClickBack.Subscribe(_ => _stackNavigationService.PopAsync().Forget());
                presenter.OnClickCancel.Subscribe(_ => _stackNavigationService.PopAsync().Forget());
                presenter.OnClickGood.Subscribe(async _ =>
                {
                    await _stackNavigationService.PopAsync();
                    _stackNavigationService.Dispatch(new ReviewEventArgs()
                    {
                        ProductId = model.ProductId.Value,
                        IsGood = true
                    });
                });
                presenter.OnClickBad.Subscribe(async _ =>
                {
                    await _stackNavigationService.PopAsync();
                    _stackNavigationService.Dispatch(new ReviewEventArgs()
                    {
                        ProductId = model.ProductId.Value,
                        IsGood = false
                    });
                });
            });
        }

        public override ScreenUIType ScreenUIType => ScreenUIType.WindowOrTransparent;
    }
}