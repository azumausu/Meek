using System;
using System.Collections.Generic;
using Meek.MVP;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Demo
{
    public class SplashPresenter : Presenter<SplashModel>
    {
        [SerializeField] private Button _signUpButton;
        [SerializeField] private Button _logInButton;

        public IObservable<Unit> OnClickSignUp => _signUpButton.OnClickAsObservable();
        public IObservable<Unit> OnClickLogIn => _logInButton.OnClickAsObservable();

        protected override IEnumerable<IDisposable> Bind(SplashModel model)
        {
            yield break;
        }
    }
}