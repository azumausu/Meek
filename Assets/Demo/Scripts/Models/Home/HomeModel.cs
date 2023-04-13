using Demo.ApplicationServices;
using UniRx;

namespace Demo
{
    public class HomeModel
    {
        private readonly GlobalStore _globalStore;
        private readonly ReactiveCollection<int> _favoriteProducts = new();
        private readonly ReactiveProperty<TabType> _selectingTab = new();
        
        public IReadOnlyReactiveProperty<TabType> SelectingTab => _selectingTab;
        public IReadOnlyReactiveCollection<int> FavoriteProducts => _globalStore.FavoriteProducts;
        
        public HomeModel(GlobalStore globalStore)
        {
            _globalStore = globalStore;
        }
        
        public void AddProduct(int productId, bool isGood)
        {
            if (isGood) _globalStore.AddFavoriteProduct(productId);
        }

        public void UpdateTab(TabType tabType)
        {
            _selectingTab.Value = tabType;
        }
    }
}