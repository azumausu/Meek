using System.Threading.Tasks;

namespace Meek.MVP
{
    public interface IPresenter<TModel> : IPresenter
    {
        Task LoadAsync(TModel model);
    }

    public interface IPresenter
    {
        void Setup();
    }
}