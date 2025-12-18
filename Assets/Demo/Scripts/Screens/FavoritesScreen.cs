using System.Threading.Tasks;
using Meek;
using Meek.MVP;
using Meek.NavigationStack;

namespace Demo
{
    public class FavoritesScreen : MVPScreen<FavoritesModel>
    {
        protected override async ValueTask<FavoritesModel> CreateModelAsync()
        {
            var globalStore = AppServices.GetService<GlobalStore>();
            return await Task.FromResult(new FavoritesModel(globalStore));
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