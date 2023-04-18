using Meek;
using Meek.MVP;
using UnityEngine;

namespace Demo
{
    public class Main : MonoBehaviour
    {
        [SerializeField] private InputLocker _inputLocker;
        [SerializeField] private PrefabViewManager _prefabViewManager;
        
        public void Start()
        {
            Application.targetFrameRate = Screen.currentResolution.refreshRate;
            var app = MVPApplication.CreateRootApp(
                new MVPRootApplicationOption()
                {
                    ContainerBuilderFactory = x => new VContainerServiceCollection(x),
                    InputLocker = _inputLocker,
                    PrefabViewManager = _prefabViewManager,
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