using System.Threading.Tasks;
using Meek.UGUI;

namespace Meek.MVP
{
    public interface IPresenterViewHandler : IPrefabViewHandler
    {
        ValueTask LoadAsync<TModel>(TModel model);

        TPresenter GetPresenter<TPresenter>() where TPresenter : IPresenter;
    }
}