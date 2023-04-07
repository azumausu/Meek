using System;
using System.Collections.Generic;
using Harmos.UI.UGUI.MVP;
using Meek.MVP;
using MVP.Models;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace MVP.Presenters
{
    public class YellowPresenter : Presenter<YellowModel>
    {
        [SerializeField] private Button _button;
        [SerializeField] private Button _exitButton;

        public IObservable<Unit> OnClick => _button.OnClickAsObservable();
        public IObservable<Unit> OnClickExit => _exitButton.OnClickAsObservable();
        
        protected override IEnumerable<IDisposable> Bind(YellowModel model)
        {
            yield break;
        }
    }
}