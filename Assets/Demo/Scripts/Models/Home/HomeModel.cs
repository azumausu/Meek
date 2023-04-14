using System.Collections.Generic;
using UniRx;

namespace Demo
{
    public class HomeModel
    {
        private readonly GlobalStore _globalStore;
        public IReadOnlyReactiveProperty<List<FavoritesProductEntity>> FavoriteProducts => _globalStore.FavoriteProducts;
        public IReadOnlyReactiveProperty<List<ProductEntity>> Products => _globalStore.Products;

        public HomeModel(GlobalStore globalStore)
        {
            _globalStore = globalStore;
        }
        
        public void AddProduct(int productId, bool isGood)
        {
            if (isGood) _globalStore.AddFavoriteProduct(productId);
        }
    }
}