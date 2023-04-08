namespace Meek.UGUI
{
    public interface IPrefabViewManager
    {
        void SortOrderInHierarchy(NavigationContext context);
        
        void AddInHierarchy(PrefabViewHandler handler);
    }
}