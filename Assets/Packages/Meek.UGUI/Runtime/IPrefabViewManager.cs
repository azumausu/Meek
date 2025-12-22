using JetBrains.Annotations;
using Meek.NavigationStack;
using UnityEngine;

#if MEEK_ENABLE_UGUI
namespace Meek.UGUI
{
    public interface IPrefabViewManager
    {
        Transform GetRootNode(IScreen ownerScreen, [CanBeNull] object param = null);

        void SortOrderInHierarchy(StackNavigationContext context);
    }
}
#endif