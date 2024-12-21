#if MEEK_ENABLE_UGUI
using System;
using System.Linq;
using Meek.NavigationStack;
using UnityEngine;

namespace Meek.UGUI
{
    public class DefaultPrefabViewManager : MonoBehaviour, IPrefabViewManager
    {
        [SerializeField] private Transform _rootNode;

        void IPrefabViewManager.AddInHierarchy(PrefabViewHandler handler)
        {
            handler.RootNode.gameObject.SetLayerRecursively(_rootNode.gameObject.layer);
            handler.RootNode.SetParent(_rootNode);
            (handler.RootNode.transform as RectTransform)!.anchoredPosition3D = Vector3.zero;
        }

        void IPrefabViewManager.SortOrderInHierarchy(NavigationContext navigationContext)
        {
            var navigationService = navigationContext.AppServices.GetService<StackNavigationService>();
            var uis = navigationService.ScreenContainer.Screens.OfType<StackScreen>().Select(x => x.UI);
            foreach (var ui in uis)
            {
                foreach (var prefabView in ui.ViewHandlers.Reverse().OfType<PrefabViewHandler>())
                {
                    prefabView.RootNode.SetAsFirstSibling();
                }
            }
        }
    }
}
#endif