using Meek.MVP;
using Meek.NavigationStack;
using UnityEngine;

namespace Demo
{
    public class PresenterLoaderFactory : IPresenterLoaderFactory
    {
        public IViewHandlerLoader CreateLoader<TModel>(TModel model, string prefabName)
        {
            Debug.Log($"Custom PresenterLoaderFactory");
            return new PresenterLoaderFromResources<TModel>(model, $"UI/{prefabName}");
        }
    }
}