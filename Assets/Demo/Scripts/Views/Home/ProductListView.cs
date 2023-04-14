using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Demo
{
    public class ProductListView : MonoBehaviour
    {
        [SerializeField] private ProductListItemView _productListItemViewPrefab;
        [SerializeField] private RectTransform _contentRoot;

        private readonly Subject<int> _clickFavoritesSubject = new ();
        private readonly List<ProductListItemView> _caches = new ();

        public IObservable<int> OnClickFavoritesProduct => _clickFavoritesSubject;

        public void UpdateView(IReadOnlyCollection<ProductEntity> products)
        {
            foreach (var cache in _caches) Destroy(cache.gameObject);
            _caches.Clear();
            
            foreach (var product in products)
            {
                var productListItemView = Instantiate(_productListItemViewPrefab, _contentRoot);
                productListItemView.UpdateView(product);
                productListItemView.OnClick.Subscribe(x => _clickFavoritesSubject.OnNext(product.Id));
                _caches.Add(productListItemView);
            }
        }
    }
}