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
        
        [SerializeField] private Button[] _productButtons;

        private Subject<int> _clickProductSubject = new Subject<int>();

        public IObservable<int> OnClickProduct => _clickProductSubject;
        
        protected override IEnumerable<IDisposable> Bind(HomeModel model)
        {
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