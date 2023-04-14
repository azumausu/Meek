using System;
using System.Collections.Generic;
using Meek.MVP;
using UnityEngine;
using UniRx;

namespace Demo
{
    public class HomePresenter : Presenter<HomeModel>
    {
        [SerializeField] private ProductListView _productListView;

        public IObservable<int> OnClickProduct => _productListView.OnClickFavoritesProduct;
        
        protected override IEnumerable<IDisposable> Bind(HomeModel model)
        {
            yield return model.Products.Subscribe(x => _productListView.UpdateView(x));
        }
    }
}