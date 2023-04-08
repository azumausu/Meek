using System;
using System.Collections.Generic;
using Meek.MVP;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Sample
{
    public class TabPresenter : Presenter<TabModel>
    {
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private TextMeshProUGUI _message;
        [SerializeField] private Button _button;

        public IObservable<Unit> OnClick => _button.OnClickAsObservable();

        protected override IEnumerable<IDisposable> Bind(TabModel model)
        {
            yield break;
        }

        protected override void OnSetup(TabModel model)
        {
            _rectTransform.anchoredPosition = model.AnchoredPosition;
            _message.text = model.Message;
        }
    }
}