using System;
using System.Collections.Generic;
using Meek.MVP;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Demo
{
    public class ReviewPresenter : Presenter<ReviewModel>
    {
        [SerializeField] private ProductImage _productImage;

        [SerializeField] private Button _goodButton;
        [SerializeField] private Button _badButton;
        [SerializeField] private Button _cancelButton;
        [SerializeField] private Button _backButton;
        
        public IObservable<Unit> OnClickGood => _goodButton.OnClickAsObservable();
        public IObservable<Unit> OnClickBad => _badButton.OnClickAsObservable();
        public IObservable<Unit> OnClickCancel => _cancelButton.OnClickAsObservable();
        public IObservable<Unit> OnClickBack => _backButton.OnClickAsObservable();

        protected override IEnumerable<IDisposable> Bind(ReviewModel model)
        {
            yield return model.ProductId.Subscribe(x => _productImage.UpdateView(x));
        }
    }
}