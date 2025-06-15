using System;
using Meek.NavigationStack;
using Meek.UGUI;

namespace Meek.MVP
{
    public class PresenterLoaderFactoryFromResources : IPresenterLoaderFactory
    {
        private readonly IPrefabViewManager _prefabViewManager;

        public PresenterLoaderFactoryFromResources(IPrefabViewManager serviceProvider)
        {
            _prefabViewManager = serviceProvider;
        }

        public IViewHandlerLoader CreateLoader<TModel>(TModel model, string prefabName)
        {
            return new PresenterLoaderFromResources<TModel>(model, _prefabViewManager, $"UI/{prefabName}");
        }
    }
}