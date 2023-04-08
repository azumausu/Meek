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
        [SerializeField] private Button _homeButton;
        [SerializeField] private Button _shopButton;
        [SerializeField] private Button _bagButton;
        [SerializeField] private Button _favoritesButton;
        [SerializeField] private Button _profileButton;
        
        [SerializeField] private Button[] _productButtons;

        private Subject<int> _clickProductSubject = new Subject<int>();

        public IObservable<int> OnClickProduct => _clickProductSubject;
        public IObservable<Unit> OnClickHome => _homeButton.OnClickAsObservable();

        protected override IEnumerable<IDisposable> Bind(HomeModel model)
        {
            yield break;
        }

        protected override void OnSetup(HomeModel model)
        {
            base.OnSetup(model);
            
            foreach (var productButton in _productButtons)
            {
                var index = Array.IndexOf(_productButtons, productButton);
                productButton.OnClickAsObservable().Subscribe(_ => _clickProductSubject.OnNext(index));
            }
        }
    }
}