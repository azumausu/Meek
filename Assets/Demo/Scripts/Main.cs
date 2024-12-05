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
            Application.targetFrameRate = Screen.currentResolution.refreshRate;
            var app = MVPApplication.CreateApp(
                new MVPApplicationOption()
                {
                    ContainerBuilderFactory = x => new VContainerServiceCollection(x),
                    InputLocker = defaultInputLocker,
                    PrefabViewManager = defaultPrefabViewManager,
                },
                x =>
                {
                    // App Services
                    x.AddSingleton<GlobalStore>();

                    // Screen
                    x.AddTransient<SplashScreen>();
                    x.AddTransient<SignUpScreen>();
                    x.AddTransient<LogInScreen>();
                    x.AddTransient<TabScreen>();
                    x.AddTransient<ReviewScreen>();
                }
            );
            app.RunAsync<SplashScreen>().Forget();
        }
    }
}