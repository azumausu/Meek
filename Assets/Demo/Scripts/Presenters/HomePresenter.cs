using System;
using System.Collections.Generic;
using Meek.MVP;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Demo
{
    public class HomePresenter : Presenter<HomeModel>
    {
        [SerializeField] private GameObjectActiveSwitcher _existCartProductIconSwitcher;
        [SerializeField] private ToggleButton _homeButton;
        [SerializeField] private ToggleButton _searchButton;
        [SerializeField] private ToggleButton _favoritesButton;
        [SerializeField] private ToggleButton _profileButton;
        
        [SerializeField] private Button[] _productButtons;

        private Subject<int> _clickProductSubject = new Subject<int>();

        public IObservable<int> OnClickProduct => _clickProductSubject;
        public IObservable<Unit> OnClickHome => _homeButton.OnClick;
        public IObservable<Unit> OnClickSearch => _searchButton.OnClick;
        public IObservable<Unit> OnClickFavorites => _favoritesButton.OnClick;
        public IObservable<Unit> OnClickProfile => _profileButton.OnClick;
        
        protected override IEnumerable<IDisposable> Bind(HomeModel model)
        {
            yield return model.SelectingTab.Subscribe(x =>
            {
                _homeButton.UpdateView(x == TabType.Home);
                _searchButton.UpdateView(x == TabType.Search);
                _favoritesButton.UpdateView(x == TabType.Favorites);
                _profileButton.UpdateView(x == TabType.Profile);
            });
            yield return model.FavoriteProducts.ObserveCountChanged().Subscribe(x =>
            {
                _existCartProductIconSwitcher.Switch(x > 0);
            });
        }

        protected override void OnSetup(HomeModel model)
        {
            base.OnSetup(model);
            
            foreach (var productButton in _productButtons)
            {
                var index = Array.IndexOf(_productButtons, productButton);
                productButton.OnClickAsObservable().Subscribe(_ => _clickProductSubject.OnNext(index));
            }
            _existCartProductIconSwitcher.Switch(model.FavoriteProducts.Count > 0);
        }
    }
}