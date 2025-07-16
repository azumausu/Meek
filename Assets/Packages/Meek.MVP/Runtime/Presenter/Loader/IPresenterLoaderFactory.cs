using Meek.NavigationStack;

namespace Meek.MVP
{
    public interface IPresenterLoaderFactory
    {
        IViewHandlerLoader CreateLoader<TModel>(IScreen ownerScreen, TModel model, string prefabName, object param);
    }
}