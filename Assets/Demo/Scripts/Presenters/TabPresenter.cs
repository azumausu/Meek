using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Meek.MVP;
using Meek.UGUI;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;

namespace Demo
{
    public class TabPresenter : Presenter<TabModel>
    {
        [SerializeField] private ToggleButton _homeToggleButton;
        [SerializeField] private CanvasGroup _homeCanvasGroup;

        [FormerlySerializedAs("_homeInputLocker")] [SerializeField]
        private DefaultInputLocker homeDefaultInputLocker;

        [FormerlySerializedAs("_homePrefabViewManager")] [SerializeField]
        private DefaultPrefabViewManager homeDefaultPrefabViewManager;

        [SerializeField] private ToggleButton _searchToggleButton;
        [SerializeField] private CanvasGroup _searchCanvasGroup;

        [FormerlySerializedAs("_searchInputLocker")] [SerializeField]
        private DefaultInputLocker searchDefaultInputLocker;

        [FormerlySerializedAs("_searchPrefabViewManager")] [SerializeField]
        private DefaultPrefabViewManager searchDefaultPrefabViewManager;

        [SerializeField] private ToggleButton _favoritesToggleButton;
        [SerializeField] private CanvasGroup _favoritesCanvasGroup;

        [FormerlySerializedAs("_favoritesInputLocker")] [SerializeField]
        private DefaultInputLocker favoritesDefaultInputLocker;

        [FormerlySerializedAs("_favoritesPrefabViewManager")] [SerializeField]
        private DefaultPrefabViewManager favoritesDefaultPrefabViewManager;

        [SerializeField] private GameObjectActiveSwitcher _badgeActiveSwitcher;

        [SerializeField] private ToggleButton _profileToggleButton;
        [SerializeField] private CanvasGroup _profileCanvasGroup;

        [FormerlySerializedAs("_profileInputLocker")] [SerializeField]
        private DefaultInputLocker profileDefaultInputLocker;

        [FormerlySerializedAs("_profilePrefabViewManager")] [SerializeField]
        private DefaultPrefabViewManager profileDefaultPrefabViewManager;

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

            yield return model.FavoriteProducts.Subscribe(x => { _badgeActiveSwitcher.Switch(x.Count(y => y.IsNew) > 0); });
        }

        protected override async Task LoadAsync(TabModel model)
        {
            var homeServices = await new VContainerServiceCollection(model.AppServices)
                .AddMeekMvp(new MvpNavigatorOptions()
                    { InputLocker = homeDefaultInputLocker, PrefabViewManager = homeDefaultPrefabViewManager })
                .BuildAndRunMeekMvpAsync<HomeScreen>();
            homeServices.AddTo(this);
            var searchServices = await new VContainerServiceCollection(model.AppServices)
                .AddMeekMvp(new MvpNavigatorOptions()
                    { InputLocker = searchDefaultInputLocker, PrefabViewManager = searchDefaultPrefabViewManager })
                .BuildAndRunMeekMvpAsync<SearchScreen>();
            searchServices.AddTo(this);
            var favoritesServices = await new VContainerServiceCollection(model.AppServices)
                .AddMeekMvp(new MvpNavigatorOptions()
                    { InputLocker = favoritesDefaultInputLocker, PrefabViewManager = favoritesDefaultPrefabViewManager })
                .BuildAndRunMeekMvpAsync<FavoritesScreen>();
            favoritesServices.AddTo(this);
            var profileServices = await new VContainerServiceCollection(model.AppServices)
                .AddMeekMvp(new MvpNavigatorOptions()
                    { InputLocker = profileDefaultInputLocker, PrefabViewManager = profileDefaultPrefabViewManager })
                .BuildAndRunMeekMvpAsync<ProfileScreen>();
            profileServices.AddTo(this);
        }
    }
}