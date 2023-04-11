# Meek
Meek is a Unity library for building user interfaces implemented on a DI basis.  
It primarily provides screen navigation and screen management functions and tools to facilitate implementation based on the MVP architecture.


https://user-images.githubusercontent.com/19426596/231222911-1acaf0f8-439b-4603-bca9-7ee6f755230b.mov
  
  
The images used in the demo are free content.  
For copyright information, please check the following websites.  
[Nucleus UI](https://www.nucleus-ui.com/)

# Requirements
- Unity 2021.3 (LTS) or newer
- uGUI

# Install
Add following two lines to Pacakges/manifest.json.  

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
It is recommended that UniRx be installed for implementation in the MVP(Model-View-Presenter) pattern.
