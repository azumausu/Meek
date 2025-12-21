#if MEEK_ENABLE_UGUI
using Meek.NavigationStack;
using UnityEngine;
using System;
using System.Linq;
using JetBrains.Annotations;

namespace Meek.UGUI
{
    public class DefaultPrefabViewManager : MonoBehaviour, IPrefabViewManager
    {
        [SerializeField] private RectTransform _rootNode;

        public virtual Transform GetRootNode(IScreen ownerScreen, [CanBeNull] object param)
        {
            return _rootNode;
        }

        public virtual void SortOrderInHierarchy(NavigationContext navigationContext)
        {
            var navigationService = navigationContext.AppServices.GetService<StackNavigationService>();
            var uis = navigationService.ScreenContainer.Screens.OfType<StackScreen>().Select(x => x.UI);
            foreach (var ui in uis)
            {
                foreach (var prefabView in ui.ViewHandlers.Reverse().OfType<DynamicPrefabViewHandler>())
                {
                    prefabView.RootNode.SetAsFirstSibling();
                }
            }
        }
    }
}
#endif