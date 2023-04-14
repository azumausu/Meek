using System;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Demo
{
    public class ProductListItemView : MonoBehaviour
    {
        [SerializeField] private ProductImage _productImage;
        
        [SerializeField] private Button _button;

        [SerializeField] private TextMeshProUGUI _name;
        
        public IObservable<Unit> OnClick => _button.OnClickAsObservable();
        
        
        public void UpdateView(ProductEntity product)
        {
            _productImage.UpdateView(product.Id);
            _name.text = product.Name;
        }
    }
}