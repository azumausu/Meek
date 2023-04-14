using System.Threading.Tasks;
using Meek.MVP;
using Meek.NavigationStack;

namespace Demo
{
    public class ProfileScreen : MVPScreen<ProfileModel>
    {
        protected override async ValueTask<ProfileModel> CreateModelAsync()
        {
            return await Task.FromResult(new ProfileModel());
        }
        
        protected override void RegisterEvents(EventHolder eventHolder, ProfileModel model)
        {
            eventHolder.ScreenWillStart(async () =>
            {
                var presenter = await LoadPresenterAsync<ProfilePresenter>();
            });
        }
    }
}
