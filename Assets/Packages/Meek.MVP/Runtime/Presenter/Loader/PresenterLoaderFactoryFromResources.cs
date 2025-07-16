using System;
using JetBrains.Annotations;
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

        public IViewHandlerLoader CreateLoader<TModel>(IScreen ownerScreen, TModel model, string prefabName, [CanBeNull] object param)
        {
            return new PresenterLoaderFromResources<TModel>(ownerScreen, model, _prefabViewManager, $"UI/{prefabName}");
        }
    }
}