using System.Threading.Tasks;
using Meek.MVP;
using Meek.NavigationStack;

namespace Demo
{
    public class FavoritesScreen : MVPScreen<FavoritesModel>
    {
        protected override async ValueTask<FavoritesModel> CreateModelAsync()
        {
            return await Task.FromResult(new FavoritesModel());
        }

        protected override void RegisterEvents(EventHolder eventHolder, FavoritesModel model)
        {
            eventHolder.ScreenWillStart(async () =>
            {
                var presenter = await LoadPresenterAsync<FavoritesPresenter>();
            });
        }
    }
}
