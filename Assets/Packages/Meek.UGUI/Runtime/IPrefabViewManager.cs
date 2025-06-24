using Meek.NavigationStack;
using UnityEngine;
using System;
using System.Linq;

#if MEEK_ENABLE_UGUI
namespace Meek.UGUI
{
    public interface IPrefabViewManager
    {
        Transform PrefabRootNode { get; }

        void SortOrderInHierarchy(NavigationContext navigationContext)
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