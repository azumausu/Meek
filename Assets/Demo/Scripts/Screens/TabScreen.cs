using System.Threading.Tasks;
using Meek.MVP;
using Meek.NavigationStack;
using UniRx;

namespace Demo
{
    public class TabScreen : MVPScreen<TabModel>
    {
        protected override async ValueTask<TabModel> CreateModelAsync()
        {
            return await Task.FromResult(new TabModel(AppServices));
        }

        protected override void RegisterEvents(EventHolder eventHolder, TabModel model)
        {
            eventHolder.ScreenWillStart(async () =>
            {
                var presenter = await LoadPresenterAsync<TabPresenter>();
            
                presenter.OnClickHome.Subscribe(_ => model.UpdateTab(TabType.Home));
                presenter.OnClickSearch.Subscribe(_ => model.UpdateTab(TabType.Search));
                presenter.OnClickFavorites.Subscribe(_ => model.UpdateTab(TabType.Favorites));
                presenter.OnClickProfile.Subscribe(_ => model.UpdateTab(TabType.Profile));
            });
        }
    }
}