using JetBrains.Annotations;
using UnityEngine;

#if MEEK_ENABLE_UGUI
namespace Meek.UGUI
{
    public interface IPrefabViewManager
    {
        Transform GetRootNode(IScreen ownerScreen, [CanBeNull] object param = null);

        void SortOrderInHierarchy(NavigationContext navigationContext);
    }
}
#endif