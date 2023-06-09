[日本語ドキュメント](README_JA.md)

# Meek
Meek is a Unity library for building user interfaces implemented on a DI basis.  
It primarily provides screen navigation and screen management functions and tools to facilitate implementation based on the MVP architecture.

[MeekDemo](https://user-images.githubusercontent.com/19426596/232242080-f2eac6e7-e1ae-48c3-9816-8aebae1f951b.mov)

The images used in the demo are free content.  
For copyright information, please check the following websites.  
[Nucleus UI](https://www.nucleus-ui.com/)

# Requirements
- Unity 2021.3 (LTS) or newer
- uGUI
- [VContainer](https://github.com/hadashiA/VContainer)

  

# Install
Add following six lines to Pacakges/manifest.json.  

```json
{
  "dependencies": {
    "jp.amatech.meek": "https://github.com/azumausu/Meek.git?path=Assets/Packages/Meek",
    "jp.amatech.meek.navigationstack": "https://github.com/azumausu/Meek.git?path=Assets/Packages/Meek.NavigationStack",
    "jp.amatech.meek.ugui": "https://github.com/azumausu/Meek.git?path=Assets/Packages/Meek.UGUI",
    "jp.amatech.meek.vcontainer": "https://github.com/azumausu/Meek.git?path=Assets/Packages/Meek.VContainer",
    "jp.amatech.meek.mvp": "https://github.com/azumausu/Meek.git?path=Assets/Packages/Meek.MVP",
    "jp.hadashikick.vcontainer": "https://github.com/hadashiA/VContainer.git?path=VContainer/Assets/VContainer"
  }
}
```
It is recommended that [UniRx](https://github.com/neuecc/UniRx) be installed for implementation in the MVP(Model-View-Presenter) pattern.

# Quick Start
#TODO

# Fundamentals
## Entry Point
```csharp
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
            var app = new MVPApplication().CreateApp<SplashScreen>(
                x => new VContainerServiceCollection(x),
                _inputLocker,
                _prefabViewManager,
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
```
MVPApplication

## Screen
#TODO

## Lifecycle
#TODO

## Navigation
### Navigation Method
#TODO
### Navigation Animation
#TODO

## MVP(Model-View-Presenter)
#TODO
