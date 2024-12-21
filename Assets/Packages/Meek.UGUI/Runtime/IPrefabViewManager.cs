#if MEEK_ENABLE_UGUI
namespace Meek.UGUI
{
    public interface IPrefabViewManager
    {
        void SortOrderInHierarchy(NavigationContext context);
        
        void AddInHierarchy(PrefabViewHandler handler);
    }
}
#endif