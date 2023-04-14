using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;

namespace Demo
{
    public class FavoritesProductListView : MonoBehaviour
    {
        [SerializeField] private FavoriteProductListItemView _favoriteProductListItemViewPrefab;
        [SerializeField] private RectTransform _contentRoot;

        private readonly Subject<int> _clickFavoritesSubject = new ();
        private readonly List<FavoriteProductListItemView> _caches = new ();

        public IObservable<int> OnClickFavoritesProduct => _clickFavoritesSubject;

        public void UpdateView(IEnumerable<FavoritesProductEntity> favoritesProductEntities)
        {
            foreach (var cache in _caches) Destroy(cache.gameObject);
            _caches.Clear();
            
            foreach (var favoritesProductEntity in favoritesProductEntities)
            {
                var productListItemView = Instantiate(_favoriteProductListItemViewPrefab, _contentRoot);
                productListItemView.UpdateView(favoritesProductEntity);
                productListItemView.OnClick.Subscribe(x => _clickFavoritesSubject.OnNext(favoritesProductEntity.Id));
                _caches.Add(productListItemView);
            }
        }
    }
}