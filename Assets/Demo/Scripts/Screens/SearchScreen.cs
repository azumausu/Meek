using System.Threading.Tasks;
using Meek.MVP;
using Meek.NavigationStack;

namespace Demo
{
    public class SearchScreen : MVPScreen<SearchModel>
    {
        protected override async ValueTask<SearchModel> CreateModelAsync()
        {
            return await Task.FromResult(new SearchModel());
        }

        protected override void RegisterEvents(EventHolder eventHolder, SearchModel model)
        {
            eventHolder.ScreenWillStart(async () =>
            {
                var presenter = await LoadPresenterAsync<SearchPresenter>();
            });
        }
    }
}
