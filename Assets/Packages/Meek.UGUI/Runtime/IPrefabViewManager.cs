using UnityEngine;

#if MEEK_ENABLE_UGUI
namespace Meek.UGUI
{
    public interface IPrefabViewManager
    {
        public Transform PrefabRootNode { get; }

        void SortOrderInHierarchy(NavigationContext context);
    }
}
#endif