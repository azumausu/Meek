using System;
using System.Linq;
using DG.Tweening;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.Pool;

namespace Demo
{
    [RequireComponent(typeof(TMP_InputField))]
    public class InputFieldView : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] private RectTransform _label;
        [SerializeField] private GraphicEnabledSwitcher _normalBgSwitcher;
        [SerializeField] private GraphicEnabledSwitcher _successIconSwitcher;
        [SerializeField] private GraphicEnabledSwitcher _errorBgSwitcher;
        [SerializeField] private GraphicEnabledSwitcher _errorIconSwitcher;

        public IObservable<string> OnEndEdit => _inputField.onEndEdit.AsObservable();

        private void Awake()
        {
            _inputField.onSelect.AsObservable().Subscribe(_ =>
            {
                var sequence = DOTween.Sequence()
                    .Append(_label.DOAnchorPosY(37, 0.3f).SetEase(Ease.InOutSine))
                    .Join(_label.DOScale(new Vector3(0.7f, 0.7f, 1.0f), 0.3f).SetEase(Ease.InOutSine))
                    .Play();
                this.OnDestroyAsObservable().Subscribe(_ => sequence?.Kill());
            });
            _inputField.onDeselect.AsObservable()
                .Where(_ => string.IsNullOrEmpty(_inputField.text))
                .Subscribe(_ =>
            {
                var sequence = DOTween.Sequence()
                    .Append(_label.DOAnchorPosY(0, 0.3f).SetEase(Ease.InOutSine))
                    .Join(_label.DOScale(new Vector3(1f, 1f, 1.0f), 0.3f).SetEase(Ease.InOutSine))
                    .Play();
                this.OnDestroyAsObservable().Subscribe(_ => sequence?.Kill());
            });
        }

        public void UpdateView(string text)
        {
            _inputField.text = text;
            
            if (string.IsNullOrEmpty(text))
            {
                _normalBgSwitcher.Switch(true);
                _successIconSwitcher.Switch(false);
                _errorBgSwitcher.Switch(false);
                _errorIconSwitcher.Switch(false); 
                return;
            }
            
            ListPool<Component>.Get(out var components);
            
            GetComponents(typeof(IInputFieldValidator), components);
            var isValid = components.OfType<IInputFieldValidator>().All(x => x.IsValid(text));
            
            _normalBgSwitcher.Switch(isValid);
            _successIconSwitcher.Switch(isValid);
            _errorBgSwitcher.Switch(!isValid);
            _errorIconSwitcher.Switch(!isValid);
            
            ListPool<Component>.Release(components);
        }
    }
}