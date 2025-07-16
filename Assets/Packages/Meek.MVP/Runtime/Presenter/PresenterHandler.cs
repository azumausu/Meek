using System;
using System.Threading.Tasks;
using Meek.UGUI;
using UnityEngine;

namespace Meek.MVP
{
    public class PresenterHandler : PrefabViewHandler
    {
        protected readonly IPresenter Presenter;

        public PresenterHandler(Transform parent, GameObject instance) : base(parent, instance)
        {
            Presenter = Instance.GetComponent<IPresenter>();
        }

        protected override void Setup()
        {
            Presenter.Setup();
        }

        public override async ValueTask DisposeAsync()
        {
            await base.DisposeAsync();

            if (Presenter is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync();
            }
        }
    }
}