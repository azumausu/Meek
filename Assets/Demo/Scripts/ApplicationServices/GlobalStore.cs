using System.Collections.Generic;
using UniRx;

namespace Demo
{
    public class GlobalStore
    {
        private readonly ReactiveProperty<List<ProductEntity>> _products = new();
        private readonly ReactiveProperty<List<FavoritesProductEntity>> _favoriteProducts = new(new());
        
        public IReadOnlyReactiveProperty<List<FavoritesProductEntity>> FavoriteProducts => _favoriteProducts;
        public IReadOnlyReactiveProperty<List<ProductEntity>> Products => _products;

        public GlobalStore()
        {
            MigrateMock();
        }
        
        public void AddFavoriteProduct(int productId)
        {
            var productEntity = _products.Value.Find(x => x.Id == productId);
            _favoriteProducts.Value.Add(new FavoritesProductEntity()
            {
                Id = _favoriteProducts.Value.Count + 1,
                ProductId = productEntity.Id,
                ProductEntity = productEntity,
                IsNew = true,
            });
            
            _favoriteProducts.SetValueAndForceNotify(_favoriteProducts.Value);
        }

        public void UpdateIsNew()
        {
            foreach (var favoritesProduct in _favoriteProducts.Value) favoritesProduct.IsNew = false;
            _favoriteProducts.SetValueAndForceNotify(_favoriteProducts.Value);
        }


        private void MigrateMock()
        {
            _products.Value = new List<ProductEntity>()
            {
                new ProductEntity() { Name = "Living Room", Id = 1, },
                new ProductEntity() { Name = "Bed Room", Id = 2, },
                new ProductEntity() { Name = "Kitchen", Id = 3, },
                new ProductEntity() { Name = "Bathroom", Id = 4, },
                new ProductEntity() { Name = "Dining Room", Id = 5, },
                new ProductEntity() { Name = "Office", Id = 6, },
            };
        }
    }
}