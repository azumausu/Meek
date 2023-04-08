using Meek.NavigationStack;

namespace Meek.MVP
{
    public interface IPresenterLoaderFactory
    {
        IViewHandlerLoader CreateLoader<TModel>(TModel model, string prefabName);
    }
}