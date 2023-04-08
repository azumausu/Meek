using System;
using System.Collections.Generic;
using Meek.MVP;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Demo
{
    public class SelectSizePresenter : Presenter<SelectSizeModel>
    {
        [SerializeField] private Button _backButton;
        
        [SerializeField] private ToggleButton _xsButton;
        [SerializeField] private ToggleButton _sButton;
        [SerializeField] private ToggleButton _mButton;
        [SerializeField] private ToggleButton _lButton;
        [SerializeField] private ToggleButton _xlButton;

        
        public IObservable<Unit> OnClickBack => _backButton.OnClickAsObservable();
        public IObservable<Unit> OnClickXS => _xsButton.OnClick;
        public IObservable<Unit> OnClickS => _sButton.OnClick;
        public IObservable<Unit> OnClickM => _mButton.OnClick;
        public IObservable<Unit> OnClickL => _lButton.OnClick;
        public IObservable<Unit> OnClickXL => _xlButton.OnClick;

        protected override IEnumerable<IDisposable> Bind(SelectSizeModel model)
        {
            yield return model.Size.Subscribe(x =>
            {
                _xsButton.UpdateView(x == Size.XS);
                _sButton.UpdateView(x == Size.S);
                _mButton.UpdateView(x == Size.M);
                _lButton.UpdateView(x == Size.L);
                _xlButton.UpdateView(x == Size.XL);
            });
        }
    }
}