using System;
using System.Threading.Tasks;
using Meek.UGUI;
using UnityEngine;

namespace Meek.MVP
{
    public class PresenterHandler : PrefabViewHandler
    {
        private readonly IPresenter _presenter;

        public PresenterHandler(IPrefabViewManager serviceProvider, GameObject prefab) : base(serviceProvider, prefab)
        {
            _presenter = Instance.GetComponent<IPresenter>();
        }

        protected override void Setup()
        {
            _presenter.Setup();
        }

        public override async ValueTask DisposeAsync()
        {
            await base.DisposeAsync();

            if (_presenter is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync();
            }
        }
    }
}