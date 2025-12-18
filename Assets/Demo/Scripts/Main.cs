using Meek;
using Meek.MVP;
using Meek.UGUI;
using UnityEngine;

namespace Demo
{
    public class Main : MonoBehaviour
    {
        [SerializeField] private DefaultInputLocker defaultInputLocker;
        [SerializeField] private DefaultPrefabViewManager defaultPrefabViewManager;

        public void Start()
        {
            var container = new VContainerServiceCollection()
                .AddMeekMvp(new MvpNavigatorOptions() { InputLocker = defaultInputLocker, PrefabViewManager = defaultPrefabViewManager });

            // Global Store
            container.ServiceCollection.AddSingleton<GlobalStore>();

            // Screen
            container.ServiceCollection.AddTransient<SplashScreen>();
            container.ServiceCollection.AddTransient<SignUpScreen>();
            container.ServiceCollection.AddTransient<LogInScreen>();
            container.ServiceCollection.AddTransient<TabScreen>();
            container.ServiceCollection.AddTransient<ReviewScreen>();
            container.ServiceCollection.AddTransient<HomeScreen>();
            container.ServiceCollection.AddTransient<SearchScreen>();
            container.ServiceCollection.AddTransient<FavoritesScreen>();
            container.ServiceCollection.AddTransient<ProfileScreen>();

            container.BuildAndRunMeekMvpAsync<SplashScreen>().Forget();
        }
    }
}