# Unity統合

MeekフレームワークはUnityエコシステムとの深い統合を提供し、uGUIとVContainerを活用した効率的なUI開発を可能にします。

## UGUI統合

### Meek.UGUIパッケージ概要

Unity uGUIとの統合機能を提供するパッケージです。

**主要コンポーネント**:
- `DefaultInputLocker` - 入力制御
- `DefaultPrefabViewManager` - UI階層管理
- `NavigatorAnimationByAnimationClip` - SimpleAnimationPlayerベースのアニメーション

### DefaultInputLocker

遷移中の入力をブロックする実装。

**場所**: `Meek.UGUI/Runtime/DefaultInputLocker.cs`

```csharp
public class DefaultInputLocker : MonoBehaviour, IInputLocker
{
    [SerializeField] private Image _inputBlocker;

    public IDisposable LockInput()
    {
        _inputBlocker.enabled = true;
        return new Disposer(() => _inputBlocker.enabled = false);
    }

    public bool IsInputLocking => _inputBlocker.enabled;
}
```

**使用例**:
```csharp
// MonoBehaviourにアタッチして使用
[SerializeField] private DefaultInputLocker inputLocker;

// MVPApplication初期化時に設定
var app = MVPApplication.CreateApp(
    new MVPApplicationOption() {
        InputLocker = inputLocker,
        // ...
    },
    // ...
);
```

### DefaultPrefabViewManager

UI階層の管理とルートノードの提供を行う管理クラス。

**場所**: `Meek.UGUI/Runtime/DefaultPrefabViewManager.cs`

```csharp
public class DefaultPrefabViewManager : MonoBehaviour, IPrefabViewManager
{
    [SerializeField] private RectTransform _rootNode;

    public virtual Transform GetRootNode(IScreen ownerScreen, [CanBeNull] object param)
    {
        return _rootNode;
    }

    public virtual void SortOrderInHierarchy(NavigationContext navigationContext)
    {
        var navigationService = navigationContext.AppServices.GetService<StackNavigationService>();
        var uis = navigationService.ScreenContainer.Screens.OfType<StackScreen>().Select(x => x.UI);
        foreach (var ui in uis)
        {
            foreach (var prefabView in ui.ViewHandlers.Reverse().OfType<PrefabViewHandler>())
            {
                prefabView.RootNode.SetAsFirstSibling();
            }
        }
    }
}
```

**注意**: プレハブのロード機能は `PresenterLoaderFromResources` クラスで直接 `Resources.LoadAsync<GameObject>()` を使用して実装されています。

## VContainer統合

### Meek.VContainerパッケージ概要

VContainerとの依存性注入統合を提供。

**主要コンポーネント**:
- `VContainerServiceCollection` - VContainerとの統合を行うIContainerBuilder実装
- `VContainerServiceProvider` - VContainerのIServiceProvider実装（IDisposable対応）
- `ServiceCollection` - 内部でサービス登録を管理

### VContainerServiceCollection

VContainerのIContainerBuilder実装。

**場所**: `Meek.VContainer/Runtime/VContainerServiceCollection.cs`

```csharp
public class VContainerServiceCollection : IContainerBuilder
{
    private IObjectResolver _parentObjectResolver;
    private ServiceCollection _serviceCollection = new();

    public IServiceCollection ServiceCollection => _serviceCollection;

    public VContainerServiceCollection(IServiceProvider parentServiceProvider = null)
    {
        if (parentServiceProvider == null) return;

        var vContainerServiceProvider = parentServiceProvider as VContainerServiceProvider;
        if (vContainerServiceProvider == null)
        {
            throw new ArgumentException("parentServiceProvider is not VContainerServiceProvider");
        }

        _parentObjectResolver = vContainerServiceProvider.ObjectResolver;
    }

    public IServiceProvider Build()
    {
        _serviceCollection.MakeReadOnly();
        Action<global::VContainer.IContainerBuilder> installer = containerBuilder =>
        {
            // Factory, Instance, Implementation 登録処理
            // （詳細は実装ファイル参照）
        };

        if (_parentObjectResolver == null)
        {
            var lifetimeScope = LifetimeScope.Create(installer);
            Object.DontDestroyOnLoad(lifetimeScope);
            return new VContainerServiceProvider(lifetimeScope.Container);
        }

        var scopedObjectResolver = _parentObjectResolver.CreateScope(installer);
        return new VContainerServiceProvider(scopedObjectResolver);
    }
}
```

### VContainerServiceProvider

VContainerのIServiceProvider実装。IDisposableを実装し、適切なリソース管理を行います。

**場所**: `Meek.VContainer/Runtime/VContainerServiceProvider.cs`

```csharp
public class VContainerServiceProvider : IServiceProvider, IDisposable
{
    public readonly IObjectResolver ObjectResolver;
    private bool _isDisposable = false;
        
    public VContainerServiceProvider(IObjectResolver objectResolver)
    {
        ObjectResolver = objectResolver;
    }
    
    public T GetService<T>()
    {
        if (_isDisposable)
            throw new InvalidOperationException("This instance is already disposed.");
        
        return ObjectResolver.Resolve<T>();
    }

    public object GetService(Type type)
    {
        if (_isDisposable)
            throw new InvalidOperationException("This instance is already disposed.");
        
        return ObjectResolver.Resolve(type);
    }
    
    public void Dispose()
    {
        if (_isDisposable) return;
        
        _isDisposable = true;
        ObjectResolver?.Dispose();
    }
}
```

### 使用例

```csharp
// VContainerとの統合
public class Main : MonoBehaviour
{
    [SerializeField] private DefaultInputLocker inputLocker;
    [SerializeField] private DefaultPrefabViewManager prefabViewManager;

    public void Start()
    {
        var app = MVPApplication.CreateApp(
            new MVPApplicationOption() {
                ContainerBuilderFactory = x => new VContainerServiceCollection(x),
                InputLocker = inputLocker,
                PrefabViewManager = prefabViewManager,
            },
            services => {
                // ビジネスロジックの登録（実際はServiceCollectionプロパティ経由）
                services.AddSingleton<IUserRepository, UserRepository>();
                services.AddSingleton<IImageCache, ImageCache>();
                
                // 画面の登録
                services.AddTransient<SplashScreen>();
                services.AddTransient<UserProfileScreen>();
                services.AddTransient<SettingsScreen>();
            }
        );

        app.RunAsync<SplashScreen>().Forget();
    }
}
```

## リソース管理

### Presenterプレハブの配置

**推奨ディレクトリ構造**:
```
Assets/
├── Resources/
│   └── UI/
│       ├── SplashPresenter.prefab
│       ├── UserProfilePresenter.prefab
│       └── SettingsPresenter.prefab
└── Scripts/
    └── Presenters/
        ├── SplashPresenter.cs
        ├── UserProfilePresenter.cs
        └── SettingsPresenter.cs
```

### 自動プレハブロード

プレハブロードは `PresenterLoaderFromResources` クラスで実装されています：

```csharp
// 型名からプレハブパスを自動推定
// UserProfilePresenter → Resources/UI/UserProfilePresenter.prefab
var presenter = await LoadPresenterAsync<UserProfilePresenter>();

// ロード処理の実際の実装（PresenterLoaderFromResourcesクラス内）
var prefab = await Resources.LoadAsync<GameObject>(path) as GameObject;
var instance = Object.Instantiate(prefab, parent);
var presenter = instance.GetComponent<IPresenter<TModel>>();
```

**IPrefabViewManagerの役割**:
- `GetRootNode()`: UI要素の親ノードを提供  
- `SortOrderInHierarchy()`: UI要素の描画順序を管理

### Addressable Asset System統合

カスタムローダーでAddressable Assetsに対応。

```csharp
public class AddressablePresenterLoaderFactory : IPresenterLoaderFactory
{
    public IViewHandlerLoader CreateLoader<TModel>(IScreen ownerScreen, TModel model, string prefabName, object param)
    {
        return new AddressablePresenterLoader<TModel>(ownerScreen, model, $"Presenters/{prefabName}");
    }
}

public class AddressablePresenterLoader<TModel> : IViewHandlerLoader
{
    private readonly string _addressableKey;

    public async Task<IViewHandler> LoadAsync(object param)
    {
        // Addressable Assetsからロード
        var handle = Addressables.LoadAssetAsync<GameObject>(_addressableKey);
        var prefab = await handle.Task;
        
        var instance = Object.Instantiate(prefab);
        var presenter = instance.GetComponent<IPresenter<TModel>>();
        
        return new AddressablePrefabViewHandler(instance, presenter, handle);
    }
}
```

## アニメーションシステム

### NavigatorAnimationByAnimationClip

SimpleAnimationPlayerを使用したAnimationClipベースのアニメーション実装。画面フィルタリングとアニメーションタイプ管理機能を持ちます。

**場所**: `Meek.UGUI/Runtime/NavigatorAnimation/NavigatorAnimationByAnimationClip.cs`

```csharp
[RequireComponent(typeof(SimpleAnimationPlayer))]
public class NavigatorAnimationByAnimationClip : MonoBehaviour, INavigatorAnimation
{
    [SerializeField] private bool _enabledFromScreenName;
    [SerializeField] private bool _enabledToScreenName;
    [SerializeField] private NavigatorAnimationType _navigatorAnimationType;
    [SerializeField] private AnimationClip _animationClip;
    [SerializeField] private string _fromScreenName;
    [SerializeField] private string _toScreenName;

    private SimpleAnimationPlayer _simpleAnimationPlayer;

    private void Awake() => _simpleAnimationPlayer = GetComponent<SimpleAnimationPlayer>();

    public NavigatorAnimationType NavigatorAnimationType => _navigatorAnimationType;

    public string FromScreenName => _enabledFromScreenName ? _fromScreenName : null;

    public string ToScreenName => _enabledToScreenName ? _toScreenName : null;

    public float Length => _animationClip.length;

    public void Evaluate(float t) => _simpleAnimationPlayer.Set(_animationClip, t);

    public void Play(Action onComplete = null) =>
        _simpleAnimationPlayer.Play(_animationClip, onEnd: () => { onComplete?.Invoke(); });

    public void Stop() => _simpleAnimationPlayer.Stop();
}
```

## 入力システム統合

### UIInputSyncer

UI要素の入力状態を同期。

**場所**: `Meek.UGUI/Runtime/UIInputSyncer.cs`

```csharp
public class UIInputSyncer : MonoBehaviour, IInputSwitcher
{
    [SerializeField] private Selectable[] _selectables;
    [SerializeField] private bool _defaultInteractable = true;

    public void SetInteractable(bool interactable)
    {
        foreach (var selectable in _selectables)
        {
            if (selectable != null)
            {
                selectable.interactable = interactable;
            }
        }
    }

    public void Reset()
    {
        SetInteractable(_defaultInteractable);
    }
}
```

### New Input System対応

```csharp
public class NewInputSystemLocker : MonoBehaviour, IInputLocker
{
    [SerializeField] private InputActionAsset _inputActions;
    private bool _isLocked;

    public IDisposable LockInput()
    {
        return new LockObject(() =>
        {
            _isLocked = true;
            _inputActions.Disable();
        }, () =>
        {
            _isLocked = false;
            _inputActions.Enable();
        });
    }
}
```

[[← Middleware System]](Middleware-System) | [[API Reference →]](API-Reference)