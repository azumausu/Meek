using System;
using System.Collections.Generic;
using Meek.MVP;
using UniRx;
using UnityEngine;

namespace Demo
{
    public class FavoritesPresenter : Presenter<FavoritesModel>
    {
        [SerializeField] private FavoritesProductListView _favoritesProductListView;
        
        protected override IEnumerable<IDisposable> Bind(FavoritesModel model)
        {
            yield return model.FavoriteProducts.Subscribe(x => _favoritesProductListView.UpdateView(x));
        }
    }
}