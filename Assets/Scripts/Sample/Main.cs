using Harmos;
using Harmos.UI.UGUI.MVP;
using Meek;
using Meek.MVP;
using UnityEngine;
using VContainer.Unity;

namespace Sample
{
    public class Main : MonoBehaviour
    {
        public IServiceProvider App;
        
        public void Start()
        {
            App = new MVPApplication().CreateApp<BlueScreen>(
                x => new VContainerServiceCollection(x),
                UIManager.I,
                UIManager.I,
                x =>
                {
                    x.AddTransient<BlueScreen>();
                    x.AddTransient<RedDialogScreen>();
                    x.AddTransient<YellowScreen>();

                    x.AddTransient<TabChildScreen>();
                }
            );
        }
    }
}