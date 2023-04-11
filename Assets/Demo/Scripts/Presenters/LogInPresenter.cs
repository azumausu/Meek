using System;
using System.Collections.Generic;
using Meek.MVP;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Demo
{
    public class LogInPresenter : Presenter<LogInModel>
    {
        [SerializeField] private InputFieldView _emailInputField;
        [SerializeField] private InputFieldView _passwordInputField;

        [SerializeField] private Button _backButton;
        [SerializeField] private Button _logInButton;

        public IObservable<Unit> OnClickBack => _backButton.OnClickAsObservable();
        public IObservable<Unit> OnClickLogIn => _logInButton.OnClickAsObservable();
        
        public IObservable<string> OnEndEditEmail => _emailInputField.OnEndEdit;
        public IObservable<string> OnEndEditPassword => _passwordInputField.OnEndEdit;


        protected override IEnumerable<IDisposable> Bind(LogInModel model)
        {
            yield return model.Email.Subscribe(x => _emailInputField.UpdateView(x));
            yield return model.Password.Subscribe(x => _passwordInputField.UpdateView(x));
        }
    }
}