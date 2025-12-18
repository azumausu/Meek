using System;
using System.Collections.Generic;
using UniRx;
using Meek;

namespace Demo
{
    public class TabModel
    {
        private readonly GlobalStore _globalStore;
        private readonly ReactiveProperty<TabType> _selectingTab = new();
        public IServiceProvider AppServices { get; }

        public IReadOnlyReactiveProperty<TabType> SelectingTab => _selectingTab;
        public IReadOnlyReactiveProperty<List<FavoritesProductEntity>> FavoriteProducts => _globalStore.FavoriteProducts;

        public TabModel(IServiceProvider serviceProvider)
        {
            AppServices = serviceProvider;
            _globalStore = serviceProvider.GetService<GlobalStore>();
        }

        public void UpdateTab(TabType tabType)
        {
            _selectingTab.Value = tabType;
            if (tabType == TabType.Favorites) UpdateIsNew();
        }

        private void UpdateIsNew()
        {
            _globalStore.UpdateIsNew();
        }
    }
}