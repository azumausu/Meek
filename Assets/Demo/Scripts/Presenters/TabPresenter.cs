using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Meek;
using Meek.MVP;
using UniRx;
using UnityEngine;

namespace Demo
{
    public class TabPresenter : Presenter<TabModel>
    {
        [SerializeField] private ToggleButton _homeToggleButton;
        [SerializeField] private CanvasGroup _homeCanvasGroup;
        [SerializeField] private InputLocker _homeInputLocker;
        [SerializeField] private PrefabViewManager _homePrefabViewManager;
        
        [SerializeField] private ToggleButton _searchToggleButton;
        [SerializeField] private CanvasGroup _searchCanvasGroup;
        [SerializeField] private InputLocker _searchInputLocker;
        [SerializeField] private PrefabViewManager _searchPrefabViewManager;
        
        [SerializeField] private ToggleButton _favoritesToggleButton;
        [SerializeField] private CanvasGroup _favoritesCanvasGroup;
        [SerializeField] private InputLocker _favoritesInputLocker;
        [SerializeField] private PrefabViewManager _favoritesPrefabViewManager;
        [SerializeField] private GameObjectActiveSwitcher _badgeActiveSwitcher;

        [SerializeField] private ToggleButton _profileToggleButton;
        [SerializeField] private CanvasGroup _profileCanvasGroup;
        [SerializeField] private InputLocker _profileInputLocker;
        [SerializeField] private PrefabViewManager _profilePrefabViewManager;
        
        public IObservable<Unit> OnClickHome => _homeToggleButton.OnClick;
        public IObservable<Unit> OnClickSearch => _searchToggleButton.OnClick;
        public IObservable<Unit> OnClickFavorites => _favoritesToggleButton.OnClick;
        public IObservable<Unit> OnClickProfile => _profileToggleButton.OnClick;

        protected override IEnumerable<IDisposable> Bind(TabModel model)
        {
            yield return model.SelectingTab.Subscribe(x =>
            {
                _homeToggleButton.UpdateView(x == TabType.Home);
                _homeCanvasGroup.alpha = x == TabType.Home ? 1 : 0;
                _homeCanvasGroup.blocksRaycasts = x == TabType.Home;
                
                _searchToggleButton.UpdateView(x == TabType.Search);
                _searchCanvasGroup.alpha = x == TabType.Search ? 1 : 0;
                _searchCanvasGroup.blocksRaycasts = x == TabType.Search;

                _favoritesToggleButton.UpdateView(x == TabType.Favorites);
                _favoritesCanvasGroup.alpha = x == TabType.Favorites ? 1 : 0;
                _favoritesCanvasGroup.blocksRaycasts = x == TabType.Favorites;
                
                _profileToggleButton.UpdateView(x == TabType.Profile);
                _profileCanvasGroup.alpha = x == TabType.Profile ? 1 : 0;
                _profileCanvasGroup.blocksRaycasts = x == TabType.Profile;
            });

            yield return model.FavoriteProducts.Subscribe(x =>
            {
                _badgeActiveSwitcher.Switch(x.Count(y => y.IsNew) > 0);
            });
        }

        protected override async Task LoadAsync(TabModel model)
        {
            await MVPApplication.CreateChildAppAsync<HomeScreen>(
                x => new VContainerServiceCollection(x),
                _homeInputLocker,
                _homePrefabViewManager,
                x =>
                {
                    x.AddTransient<HomeScreen>();
                },
                model.AppServices
            ).AddTo(this);
            
            await MVPApplication.CreateChildAppAsync<SearchScreen>(
                x => new VContainerServiceCollection(x),
                _searchInputLocker,
                _searchPrefabViewManager,
                x =>
                {
                    x.AddTransient<SearchScreen>();
                },
                model.AppServices
            ).AddTo(this);
            
            await MVPApplication.CreateChildAppAsync<FavoritesScreen>(
                x => new VContainerServiceCollection(x),
                _favoritesInputLocker,
                _favoritesPrefabViewManager,
                x =>
                {
                    x.AddTransient<FavoritesScreen>();
                },
                model.AppServices
            ).AddTo(this);

            await MVPApplication.CreateChildAppAsync<ProfileScreen>(
                x => new VContainerServiceCollection(x),
                _profileInputLocker,
                _profilePrefabViewManager,
                x =>
                {
                    x.AddTransient<ProfileScreen>();
                },
                model.AppServices
            ).AddTo(this);
        }
    }
}