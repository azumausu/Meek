using System;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Demo
{
    public class FavoriteProductListItemView : MonoBehaviour
    {
        [SerializeField] private ProductImage _productImage;
        
        [SerializeField] private Button _button;

        [SerializeField] private TextMeshProUGUI _name;
        
        public IObservable<Unit> OnClick => _button.OnClickAsObservable();
        public void UpdateView(FavoritesProductEntity favoritesProduct)
        {
            _productImage.UpdateView(favoritesProduct.ProductId);
            _name.text = favoritesProduct.ProductEntity.Name;
        }
    }
}