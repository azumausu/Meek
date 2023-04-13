using System;
using DG.Tweening;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace Demo
{
    public class ToggleButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private CanvasGroup _activeImage;
        [SerializeField] private CanvasGroup _inactiveImage;

        public IObservable<Unit> OnClick => _button.OnClickAsObservable();

        public void UpdateView(bool isOn)
        {
            if (isOn)
            {
                var sequence = DOTween.Sequence()
                    .Append(_activeImage.DOFade(1f, 0.2f).SetEase(Ease.InOutSine))
                    .Join(_inactiveImage.DOFade(0f, 0.2f).SetEase(Ease.InOutSine))
                    .Play();
                this.OnDestroyAsObservable().Subscribe(_ => sequence?.Kill());
            }
            else
            {
                var sequence = DOTween.Sequence()
                    .Append(_activeImage.DOFade(0f, 0.2f).SetEase(Ease.InOutSine))
                    .Join(_inactiveImage.DOFade(1f, 0.2f).SetEase(Ease.InOutSine))
                    .Play(); 
                this.OnDestroyAsObservable().Subscribe(_ => sequence?.Kill());
            }
        }
    }
}