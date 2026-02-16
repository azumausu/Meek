[Japanese Documentation (日本語)](README_JA.md)

# Meek

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE.md)
[![Unity](https://img.shields.io/badge/Unity-6000.0%2B-blue.svg)](https://unity.com/)
[![Version](https://img.shields.io/badge/version-1.6.3-green.svg)](https://github.com/azumausu/Meek/releases)

A DI-based screen management framework for Unity with stack navigation and MVP architecture support.

[MeekDemo](https://user-images.githubusercontent.com/19426596/232242080-f2eac6e7-e1ae-48c3-9816-8aebae1f951b.mov)

> The images used in the demo are free content from [Nucleus UI](https://www.nucleus-ui.com/).

---

## Table of Contents

- [Features](#features)
- [Architecture Overview](#architecture-overview)
- [Requirements](#requirements)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Core Concepts](#core-concepts)
  - [Screen (MVP Pattern)](#screen-mvp-pattern)
  - [Navigation](#navigation)
  - [Screen Lifecycle](#screen-lifecycle)
  - [DI Integration](#di-integration)
- [Usage](#usage)
  - [Creating a Screen](#creating-a-screen)
  - [Passing Parameters Between Screens](#passing-parameters-between-screens)
  - [Inter-Screen Communication (Dispatch)](#inter-screen-communication-dispatch)
  - [Transition Animations](#transition-animations)
- [Advanced Usage](#advanced-usage)
  - [Nested Navigation (Tabs)](#nested-navigation-tabs)
  - [Loading Presenter Prefabs via Addressables](#loading-presenter-prefabs-via-addressables)
  - [Multiple Navigators](#multiple-navigators)
  - [Hooking into Navigation Events](#hooking-into-navigation-events)
  - [Custom DI Container](#custom-di-container)
- [API Reference](#api-reference)
- [FAQ](#faq)
- [License](#license)

---

## Features

- **Stack-based Navigation** — Push, Pop, Insert, Remove, and BackTo operations with type-safe APIs
- **MVP Architecture** — Built-in Model-View-Presenter pattern with automatic Presenter loading and reactive data binding
- **DI Container Integration** — Abstracted DI layer with VContainer adapter; screens are resolved via DI and support constructor injection
- **Transition Animations** — Configurable Open/Close/Show/Hide animations with CrossFade support and Strategy pattern extensibility
- **Screen Lifecycle Events** — Rich lifecycle hooks (WillStart, DidStart, WillPause, DidPause, WillResume, DidResume, WillDestroy, DidDestroy)
- **Input Locking** — Automatic input blocking during transitions prevents double-tap issues
- **No Static Classes** — Instance-based design allows multiple independent navigators to coexist
- **Inter-Screen Communication** — Dispatch events across the screen stack without tight coupling

---

## Architecture Overview

Meek is organized into five modular packages:

```
┌─────────────────────────────────────────────────────────┐
│                    Your Application                      │
├───────────────┬───────────────────┬─────────────────────┤
│   Meek.MVP    │    Meek.UGUI      │  Meek.VContainer    │
├───────────────┴───────────────────┴─────────────────────┤
│                  Meek.NavigationStack                     │
├─────────────────────────────────────────────────────────┤
│                      Meek (Core)                         │
└─────────────────────────────────────────────────────────┘
```

| Package | Description |
|---------|-------------|
| **Meek** | Core interfaces and abstractions (`IScreen`, `INavigator`, `IServiceCollection`) |
| **Meek.NavigationStack** | Stack-based navigation, lifecycle events, animation strategies, input locking |
| **Meek.MVP** | MVP pattern support (`MVPScreen<TModel>`, `Presenter<TModel>`, auto-loading) |
| **Meek.UGUI** | Unity uGUI integration (animation components, `DefaultInputLocker`, `DefaultPrefabViewManager`) |
| **Meek.VContainer** | VContainer DI adapter (`VContainerServiceCollection`, `VContainerServiceProvider`) |

---

## Requirements

- **Unity 6000.0** (Unity 6) or newer
- **[VContainer](https://github.com/hadashiA/VContainer)** 1.13.2+
- **[UniRx](https://github.com/neuecc/UniRx)** (recommended for MVP reactive bindings)

---

## Installation

Add the following lines to your `Packages/manifest.json`:

```json
{
  "dependencies": {
    "jp.amatech.meek": "https://github.com/azumausu/Meek.git?path=Assets/Packages",
    "jp.hadashikick.vcontainer": "https://github.com/hadashiA/VContainer.git?path=VContainer/Assets/VContainer"
  }
}
```
---

## Quick Start

### 1. Create the Entry Point

Attach this `MonoBehaviour` to a GameObject in your scene. Place `DefaultInputLocker` and `DefaultPrefabViewManager` components on the GameObject and assign them in the Inspector.

```csharp
using Meek;
using Meek.MVP;
using Meek.UGUI;
using UnityEngine;

public class Main : MonoBehaviour
{
    [SerializeField] private DefaultInputLocker defaultInputLocker;
    [SerializeField] private DefaultPrefabViewManager defaultPrefabViewManager;

    public void Start()
    {
        var container = new VContainerServiceCollection()
            .AddMeekMvp(new MvpNavigatorOptions()
            {
                InputLocker = defaultInputLocker,
                PrefabViewManager = defaultPrefabViewManager
            });

        // Register screens
        container.ServiceCollection.AddTransient<SplashScreen>();
        container.ServiceCollection.AddTransient<HomeScreen>();

        // Build and navigate to the first screen
        container.BuildAndRunMeekMvpAsync<SplashScreen>().Forget();
    }
}
```

### 2. Create a Model

```csharp
public class SplashModel
{
    // Add reactive properties here if needed
}
```

### 3. Create a Screen

```csharp
using System.Threading.Tasks;
using Meek.MVP;
using Meek.NavigationStack;
using UniRx;

public class SplashScreen : MVPScreen<SplashModel>
{
    protected override async ValueTask<SplashModel> CreateModelAsync()
    {
        return await Task.FromResult(new SplashModel());
    }

    protected override void RegisterEvents(EventHolder eventHolder, SplashModel model)
    {
        eventHolder.ScreenWillStart(async () =>
        {
            var presenter = await LoadPresenterAsync<SplashPresenter>();

            presenter.OnClickStart.Subscribe(_ => PushNavigation.PushForget<HomeScreen>());
        });
    }
}
```

### 4. Create a Presenter

Place the Presenter prefab in `Resources/UI/SplashPresenter`.

```csharp
using System;
using System.Collections.Generic;
using Meek.MVP;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class SplashPresenter : Presenter<SplashModel>
{
    [SerializeField] private Button _startButton;

    public IObservable<Unit> OnClickStart => _startButton.OnClickAsObservable();

    protected override IEnumerable<IDisposable> Bind(SplashModel model)
    {
        yield break; // No reactive bindings needed for this simple screen
    }
}
```

### 5. Run the Demo

Open `Assets/Demo/Scenes/Demo.unity` and press Play to see a full working example with 9 screens demonstrating all navigation patterns.

---

## Core Concepts

### Screen (MVP Pattern)

Meek uses a modified MVP (Model-View-Presenter) pattern where the **Screen** acts as the coordinator:

```
┌──────────────────┐      ┌───────────┐      ┌──────────────┐
│      Screen       │─────>│   Model   │<─────│  Presenter   │
│  (Coordinator)    │      │  (State)  │      │ (View+Bind)  │
└────────┬─────────┘      └───────────┘      └──────────────┘
         │                                          ▲
         │  Creates Model                           │
         │  Loads Presenter ────────────────────────┘
         │  Registers Lifecycle Events
         │
         ▼
  Navigation API (Push/Pop/etc.)
```

- **Screen** (`MVPScreen<TModel>`) — Creates the Model, loads Presenters, registers lifecycle events, and handles navigation
- **Model** — Holds screen state using `ReactiveProperty<T>` for observable data
- **Presenter** (`Presenter<TModel>`) — A Unity prefab (`MonoBehaviour`) that binds Model data to UI elements via the `Bind()` method

### Navigation

Meek provides five navigation operations:

| Operation | Method | Description |
|-----------|--------|-------------|
| **Push** | `PushNavigation.PushAsync<T>()` | Add a screen to the top of the stack |
| **Pop** | `PopNavigation.PopAsync()` | Remove the top screen from the stack |
| **Insert** | `InsertNavigation.InsertScreenBeforeAsync<TBefore, TInsert>()` | Insert a screen before a specified screen |
| **Remove** | `RemoveNavigation.RemoveAsync<T>()` | Remove a specific screen from the stack |
| **BackTo** | `BackToNavigation.BackToAsync<T>()` | Pop all screens until the specified screen is on top |

Each navigation builder supports method chaining:

```csharp
// Push with parameter and cross-fade animation
PushNavigation
    .NextScreenParameter(new MyParam { Id = 42 })
    .IsCrossFade(true)
    .PushAsync<DetailScreen>();

// Pop with skip animation
PopNavigation
    .SkipAnimation(true)
    .PopAsync();

// Fire-and-forget variants
PushNavigation.PushForget<NextScreen>();
PopNavigation.PopForget();
```

### Screen Lifecycle

```
Push                                              Pop
 │                                                 │
 ▼                                                 ▼
ScreenWillStart ──> ScreenDidStart          ScreenWillDestroy ──> ScreenDidDestroy
                         │                         ▲
                         ▼                         │
                    ScreenWillPause ─────> ScreenWillResume
                    ScreenDidPause          ScreenDidResume
                         │                         ▲
                    (another Push)           (that Pop)
```

**Animation Events** are fired around transitions:

| Event | Timing |
|-------|--------|
| `ViewWillOpen` | Before the open animation starts |
| `ViewDidOpen` | After the open animation completes |
| `ViewWillClose` | Before the close animation starts |
| `ViewDidClose` | After the close animation completes |

### DI Integration

Meek abstracts DI through `IContainerBuilder` / `IServiceCollection` / `IServiceProvider`. The VContainer adapter maps directly:

```csharp
// Register a singleton (shared across all screens)
container.ServiceCollection.AddSingleton<GlobalStore>();

// Register a screen (new instance per resolution)
container.ServiceCollection.AddTransient<HomeScreen>();

// Constructor injection works automatically
public class HomeScreen : MVPScreen<HomeModel>
{
    private readonly GlobalStore _globalStore;

    public HomeScreen(GlobalStore globalStore)  // Injected by DI
    {
        _globalStore = globalStore;
    }
}
```

---

## Usage

### Creating a Screen

#### Model with Reactive Properties

```csharp
using UniRx;

public class LogInModel
{
    private ReactiveProperty<string> _email = new();
    private ReactiveProperty<string> _password = new();

    public IReadOnlyReactiveProperty<string> Email => _email;
    public IReadOnlyReactiveProperty<string> Password => _password;

    public void UpdateEmail(string value) => _email.Value = value;
    public void UpdatePassword(string value) => _password.Value = value;
}
```

#### Screen with Reactive Bindings

```csharp
public class LogInScreen : MVPScreen<LogInModel>
{
    protected override async ValueTask<LogInModel> CreateModelAsync()
    {
        return await Task.FromResult(new LogInModel());
    }

    protected override void RegisterEvents(EventHolder eventHolder, LogInModel model)
    {
        eventHolder.ScreenWillStart(async () =>
        {
            var presenter = await LoadPresenterAsync<LogInPresenter>();

            presenter.OnClickBack.Subscribe(_ => PopNavigation.PopAsync().Forget());
            presenter.OnClickLogIn.Subscribe(_ => PushNavigation.PushAsync<TabScreen>().Forget());

            presenter.OnEndEditEmail.Subscribe(model.UpdateEmail);
            presenter.OnEndEditPassword.Subscribe(model.UpdatePassword);
        });
    }
}
```

#### Presenter with Data Binding

```csharp
public class LogInPresenter : Presenter<LogInModel>
{
    [SerializeField] private TMP_InputField _emailInputField;
    [SerializeField] private TMP_InputField _passwordInputField;
    [SerializeField] private Button _backButton;
    [SerializeField] private Button _logInButton;

    public IObservable<Unit> OnClickBack => _backButton.OnClickAsObservable();
    public IObservable<Unit> OnClickLogIn => _logInButton.OnClickAsObservable();
    public IObservable<string> OnEndEditEmail => _emailInputField.onEndEdit.AsObservable();
    public IObservable<string> OnEndEditPassword => _passwordInputField.onEndEdit.AsObservable();

    protected override IEnumerable<IDisposable> Bind(LogInModel model)
    {
        yield return model.Email.Subscribe(x => _emailInputField.text = x);
        yield return model.Password.Subscribe(x => _passwordInputField.text = x);
    }
}
```

### Passing Parameters Between Screens

Use `MVPScreen<TModel, TParam>` to accept typed parameters:

```csharp
// Define a parameter class
public class ReviewScreenParameter
{
    public int ProductId;
}

// Screen that receives the parameter
public class ReviewScreen : MVPScreen<ReviewModel, ReviewScreenParameter>
{
    protected override async ValueTask<ReviewModel> CreateModelAsync(ReviewScreenParameter parameter)
    {
        return await Task.FromResult(new ReviewModel(parameter.ProductId));
    }

    protected override void RegisterEvents(EventHolder eventHolder, ReviewModel model) { /* ... */ }
}

// Push with parameter (from another screen)
PushNavigation
    .NextScreenParameter(new ReviewScreenParameter { ProductId = 42 })
    .PushAsync<ReviewScreen>();
```

### Inter-Screen Communication (Dispatch)

Send events back to screens lower in the stack without tight coupling:

```csharp
// Define an event args class
public class ReviewEventArgs
{
    public int ProductId;
    public bool IsGood;
}

// Sending screen (ReviewScreen) — dispatches after popping
presenter.OnClickGood.Subscribe(async _ =>
{
    await PopNavigation.PopAsync();
    Dispatch(new ReviewEventArgs { ProductId = model.ProductId.Value, IsGood = true });
});

// Receiving screen (HomeScreen) — subscribes to the event
eventHolder.SubscribeDispatchEvent<ReviewEventArgs>(args =>
{
    model.AddProduct(args.ProductId, args.IsGood);
    return true; // Stop propagation
});
```

### Transition Animations

Meek supports four animation types: **Open**, **Close**, **Show**, **Hide**.

Control animation behavior per navigation:

```csharp
// Cross-fade: old and new screens animate simultaneously
PushNavigation.IsCrossFade(true).PushAsync<NextScreen>();

// Skip animation entirely
PushNavigation.SkipAnimation(true).PushAsync<NextScreen>();
```

#### Modal / Transparent Screens

Override `ScreenUIType` for screens that don't cover the full screen:

```csharp
public class ReviewScreen : MVPScreen<ReviewModel, ReviewScreenParameter>
{
    public override ScreenUIType ScreenUIType => ScreenUIType.WindowOrTransparent;
    // ...
}
```

---

## Advanced Usage

### Nested Navigation (Tabs)

Create independent navigators for each tab by instantiating separate `VContainerServiceCollection` instances. Pass the parent `IServiceProvider` to share singletons (like `GlobalStore`) across child navigators:

```csharp
// In your Presenter's LoadAsync() method
protected override async Task LoadAsync(TabModel model)
{
    var homeServices = await new VContainerServiceCollection(model.AppServices)
        .AddMeekMvp(new MvpNavigatorOptions()
        {
            InputLocker = homeDefaultInputLocker,
            PrefabViewManager = homeDefaultPrefabViewManager
        })
        .BuildAndRunMeekMvpAsync<HomeScreen>();

    // Dispose child navigator when this Presenter is destroyed
    if (homeServices is IDisposable disposable)
        disposable.AddTo(this); // UniRx AddTo extension
}
```

Each tab gets its own navigation stack, input locker, and lifecycle — completely independent. See `Assets/Demo/Scripts/Presenters/TabPresenter.cs` for a full example with four nested navigators.

### Loading Presenter Prefabs via Addressables

By default, Presenter prefabs are loaded from `Resources/UI/` via `PresenterViewProviderFromResources`. To load from Addressables instead, implement `IPresenterViewProvider` (which extends `IPrefabViewProvider`):

```csharp
public class PresenterLoaderProviderFromAddressable : IPresenterViewProvider, IDisposable
{
    private string _prefabName;
    private AsyncOperationHandle<GameObject> _asyncOperationHandle;

    public void SetPrefabName(string prefabName)
    {
        _prefabName = prefabName;
    }

    public async ValueTask<GameObject> ProvideAsync(IScreen ownerScreen, object param = null)
    {
        _asyncOperationHandle = Addressables.LoadAssetAsync<GameObject>(_prefabName);
        var prefab = await _asyncOperationHandle.Task;

        return prefab;
    }

    public void Dispose()
    {
        if (_asyncOperationHandle.IsValid())
        {
            _asyncOperationHandle.Release();
        }
    }
}
```

Register it in place of the default provider:

```csharp
appContainer.ServiceCollection.AddTransient<IPresenterViewProvider, PresenterLoaderProviderFromAddressable>();
```
Note that calling `AddTransient` twice for the same interface will override the previous registration.

### Multiple Navigators

Since Meek uses no static classes, you can create multiple independent navigation stacks:

```csharp
// Navigator A
var containerA = new VContainerServiceCollection()
    .AddMeekMvp(optionsA);
containerA.BuildAndRunMeekMvpAsync<ScreenA>();

// Navigator B (completely independent)
var containerB = new VContainerServiceCollection()
    .AddMeekMvp(optionsB);
containerB.BuildAndRunMeekMvpAsync<ScreenB>();
```

### Hooking into Navigation Events

Subscribe to navigation events for analytics, logging, or custom logic:

```csharp
var navigationService = appServices.GetService<StackNavigationService>();

navigationService.OnWillNavigate += context =>
{
    Debug.Log($"Navigating: {context.NavigatingSourceType}");
    return ValueTask.CompletedTask;
};

navigationService.OnDidNavigate += context =>
{
    Debug.Log($"Navigation complete");
    return ValueTask.CompletedTask;
};
```

### Custom DI Container

Implement `IContainerBuilder` and `IServiceProvider` to use a different DI framework:

```csharp
public class ZenjectServiceCollection : IContainerBuilder
{
    public IServiceCollection ServiceCollection { get; }

    public IServiceProvider Build()
    {
        // Map ServiceCollection to Zenject bindings
        // Return a ZenjectServiceProvider wrapper
    }
}
```

---

## API Reference

### Core Interfaces (Meek)

| Interface | Description |
|-----------|-------------|
| `IScreen` | Base screen interface with `Initialize(NavigationContext)` |
| `INavigator` | Navigation orchestrator with `NavigateAsync(NavigationContext)` |
| `IScreenContainer` | Manages the screen collection |
| `IServiceCollection` | DI service registration |
| `IServiceProvider` | DI service resolution |
| `IContainerBuilder` | Builds `IServiceProvider` from `IServiceCollection` |
| `IMiddleware` | Middleware interface with `InvokeAsync(NavigationContext, NavigationDelegate)` |

### Navigation (Meek.NavigationStack)

| Class | Description |
|-------|-------------|
| `StackNavigationService` | Main navigation API with Push/Pop/Insert/Remove/Dispatch |
| `PushNavigation` | Builder for push operations |
| `PopNavigation` | Builder for pop operations |
| `InsertNavigation` | Builder for insert operations |
| `RemoveNavigation` | Builder for remove operations |
| `BackToNavigation` | Builder for back-to operations |
| `StackScreenContainer` | LIFO screen stack implementation |
| `StackScreen` | Abstract base class for stack-managed screens |
| `IInputLocker` | Input lock control during transitions |

### MVP (Meek.MVP)

| Class | Description |
|-------|-------------|
| `MVPScreen<TModel>` | Screen with model creation and lifecycle |
| `MVPScreen<TModel, TParam>` | Screen with typed parameter support |
| `Presenter<TModel>` | MonoBehaviour-based view with `Bind(TModel)` |
| `IPresenterViewProvider` | Interface for custom Presenter prefab loading (e.g., Addressables) |
| `MvpNavigatorOptions` | Configuration for InputLocker and PrefabViewManager |

### Lifecycle Events

| Event | Trigger |
|-------|---------|
| `ScreenWillStart` / `ScreenDidStart` | Screen initialization (Push) |
| `ScreenWillPause` / `ScreenDidPause` | Screen deactivation (another Push on top) |
| `ScreenWillResume` / `ScreenDidResume` | Screen reactivation (Pop above) |
| `ScreenWillDestroy` / `ScreenDidDestroy` | Screen destruction (Pop / Remove) |
| `ViewWillOpen` / `ViewDidOpen` | Open animation start / end |
| `ViewWillClose` / `ViewDidClose` | Close animation start / end |

---

## FAQ

### Which DI container should I use?

Meek ships with a **VContainer** adapter. VContainer is lightweight and Unity-optimized, making it the recommended choice. You can implement support for other DI frameworks (e.g., Zenject) by creating a custom `IContainerBuilder`.

### Are screens Prefabs or Scenes?

**Presenters are Prefabs**, placed in `Resources/UI/` by default. Screens themselves are plain C# classes resolved through DI — they are not MonoBehaviours. This separation keeps your logic testable and framework-independent.

### How do I separate View logic from business logic?

The MVP pattern handles this naturally:
- **Model** — Pure C# state, no Unity dependencies
- **Presenter** — MonoBehaviour with serialized UI references; only binds data from Model
- **Screen** — Coordinates Model creation, Presenter loading, navigation, and lifecycle events

### Can I use multiple navigation stacks simultaneously?

Yes. Since Meek uses no static classes, each `VContainerServiceCollection().AddMeekMvp(...)` creates a fully independent navigator. The demo's `TabPresenter` shows this pattern with four nested navigators for tab content.

---

## License

Meek is licensed under the [MIT License](LICENSE.md).

Copyright (c) 2023 Hikaru Amano
