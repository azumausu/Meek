using System.Collections.Generic;
using UniRx;

namespace Demo.ApplicationServices
{
    public class GlobalStore
    {
        private readonly ReactiveCollection<int> _favoriteProducts = new();
        
        public IReadOnlyReactiveCollection<int> FavoriteProducts => _favoriteProducts;
        
        public void AddFavoriteProduct(int productId)
        {
            _favoriteProducts.Add(productId);
        }
    }
}