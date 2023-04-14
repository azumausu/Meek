using System.Collections.Generic;
using UniRx;

namespace Demo
{
    public class FavoritesModel
    {
        private readonly GlobalStore _globalStore;

        public IReadOnlyReactiveProperty<List<FavoritesProductEntity>> FavoriteProducts => _globalStore.FavoriteProducts;

        public FavoritesModel(GlobalStore globalStore)
        {
            _globalStore = globalStore;
        }
    }
}
