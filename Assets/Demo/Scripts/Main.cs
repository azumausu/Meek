using Demo;
using Meek;
using Meek.MVP;
using UnityEngine;

namespace Sample
{
    public class Main : MonoBehaviour
    {
        public IServiceProvider App;
        
        public void Start()
        {
            Application.targetFrameRate = Screen.currentResolution.refreshRate;
            App = new MVPApplication().CreateApp<SplashScreen>(
                x => new VContainerServiceCollection(x),
                UIManager.I,
                UIManager.I,
                x =>
                {
                    x.AddTransient<SplashScreen>();
                    x.AddTransient<SignUpScreen>();
                    x.AddTransient<LogInScreen>();
                    x.AddTransient<HomeScreen>();
                    x.AddTransient<ReviewScreen>();
                }
            );
        }
    }
}