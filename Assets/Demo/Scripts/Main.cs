using Demo.ApplicationServices;
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
            var app = new MVPApplication().CreateApp<SplashScreen>(
                x => new VContainerServiceCollection(x),
                _inputLocker,
                _prefabViewManager,
                x =>
                {
                    // App Services
                    x.AddSingleton<GlobalStore>();
                    
                    // Screen
                    x.AddTransient<SplashScreen>();
                    x.AddTransient<SignUpScreen>();
                    x.AddTransient<LogInScreen>();
                    x.AddTransient<HomeScreen>(x => new HomeScreen(x));
                    x.AddTransient<ReviewScreen>();
                }
            );
        }
    }
}