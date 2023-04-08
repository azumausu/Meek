using UniRx;

namespace Demo
{
    public class HomeModel
    {
        private readonly ReactiveCollection<int> _productsInCart = new();
        private readonly ReactiveProperty<TabType> _selectingTab = new();
        
        public IReadOnlyReactiveProperty<TabType> SelectingTab => _selectingTab;
        public IReadOnlyReactiveCollection<int> ProductsInCart => _productsInCart;
        
        public void AddProduct(int productId)
        {
            _productsInCart.Add(productId);
        }

        public void UpdateTab(TabType tabType)
        {
            _selectingTab.Value = tabType;
        }
    }
}