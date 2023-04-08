using Meek.NavigationStack;

namespace Meek.MVP
{
    public class PresenterLoaderFactoryFromResources : IPresenterLoaderFactory
    {
        public IViewHandlerLoader CreateLoader<TModel>(TModel model, string prefabName)
        {
            return new PresenterLoaderFromResources<TModel>(model, $"UI/{prefabName}");
        } 
    }
}