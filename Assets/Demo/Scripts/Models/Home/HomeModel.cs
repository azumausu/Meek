using UniRx;

namespace Demo
{
    public class HomeModel
    {
        private readonly ReactiveCollection<int> _favoriteProducts = new();
        private readonly ReactiveProperty<TabType> _selectingTab = new();
        
        public IReadOnlyReactiveProperty<TabType> SelectingTab => _selectingTab;
        public IReadOnlyReactiveCollection<int> FavoriteProducts => _favoriteProducts;
        
        public void AddProduct(int productId, bool isGood)
        {
            if (isGood) _favoriteProducts.Add(productId);
        }

        public void UpdateTab(TabType tabType)
        {
            _selectingTab.Value = tabType;
        }
    }
}