using System;
using System.Collections.Generic;
using Meek.MVP;
using MVP.Models;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace MVP.Presenters
{
    public class BluePresenter : Presenter<BlueModel>
    {
        [SerializeField] private Button _button;
        [SerializeField] private Button _exitButton;

        public IObservable<Unit> OnClick => _button.OnClickAsObservable();
        public IObservable<Unit> OnClickExit => _exitButton.OnClickAsObservable();
        protected override IEnumerable<IDisposable> Bind(BlueModel model)
        {
            yield break;
        }
    }
}