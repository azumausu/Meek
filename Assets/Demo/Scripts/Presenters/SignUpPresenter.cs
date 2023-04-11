using System;
using System.Collections.Generic;
using Meek.MVP;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Demo
{
    public class SignUpPresenter : Presenter<SignUpModel>
    {
        [SerializeField] private InputFieldView _nameInputField;
        [SerializeField] private InputFieldView _emailInputField;
        [SerializeField] private InputFieldView _passwordInputField;

        [SerializeField] private Button _backButton;
        [SerializeField] private Button _logInButton;
        [SerializeField] private Button _signUpButton;
        
        public IObservable<Unit> OnClickBack => _backButton.OnClickAsObservable();
        public IObservable<Unit> OnClickSignUp => _signUpButton.OnClickAsObservable();
        public IObservable<Unit> OnClickLogIn => _logInButton.OnClickAsObservable();
        
        public IObservable<string> OnEndEditName => _nameInputField.OnEndEdit;
        public IObservable<string> OnEndEditEmail => _emailInputField.OnEndEdit;
        public IObservable<string> OnEndEditPassword => _passwordInputField.OnEndEdit;


        protected override IEnumerable<IDisposable> Bind(SignUpModel model)
        {
            yield return model.Name.Subscribe(x => _nameInputField.UpdateView(x));
            yield return model.Email.Subscribe(x => _emailInputField.UpdateView(x));
            yield return model.Password.Subscribe(x => _passwordInputField.UpdateView(x));
        }
    }
}