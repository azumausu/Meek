# MVPアーキテクチャ

MeekのMVPアーキテクチャは、Model-View-Presenterパターンを Unity UI開発に最適化して実装しています。

## MVPApplication

### アプリケーションエントリポイント

`MVPApplication`はフレームワークの初期化とアプリケーション起動を担当します。

**場所**: `Meek.MVP/Runtime/MVPApplication.cs:10`

```csharp
public class MVPApplication : IDisposable
{
    private readonly IServiceProvider _appServices;
    public IServiceProvider AppServices => _appServices;

    private MVPApplication(IServiceProvider appServices)
    {
        _appServices = appServices;
    }

    public Task RunAsync<TBootScreen>() where TBootScreen : IScreen
    {
        return _appServices.GetService<PushNavigation>().PushAsync<TBootScreen>();
    }
    
    public static MVPApplication CreateApp(MVPApplicationOption option, Action<IServiceCollection> configure)
    {
        // パラメーター検証
        if (option == null) throw new ArgumentNullException(nameof(option));
        if (option.ContainerBuilderFactory == null) throw new ArgumentNullException(nameof(option.ContainerBuilderFactory));
        if (option.InputLocker == null) throw new ArgumentNullException(nameof(option.InputLocker));
        if (option.PrefabViewManager == null) throw new ArgumentNullException(nameof(option.PrefabViewManager));

        // Navigator構築
        var navigator = new NavigatorBuilder(option.ContainerBuilderFactory(option.Parent))
            .ConfigureServices(serviceCollection =>
            {
                serviceCollection.AddSingleton<ICoroutineRunner, CoroutineRunner>();
                serviceCollection.AddSingleton<IScreenContainer, StackScreenContainer>();
                serviceCollection.AddSingleton<IPresenterLoaderFactory, PresenterLoaderFactoryFromResources>();

                serviceCollection.AddDebug(new NavigationStackDebugOption() { DisplayName = "Meek Debugger", });
                serviceCollection.AddScreenNavigatorEvent();
                serviceCollection.AddInputLocker(option.InputLocker);
                serviceCollection.AddScreenUI();
                serviceCollection.AddNavigatorAnimation();
                serviceCollection.AddUGUI(option.PrefabViewManager);
                serviceCollection.AddScreenLifecycleEvent();
            }).Configure(app =>
            {
                app.UseDebug();
                app.UseScreenNavigatorEvent();
                app.UseInputLocker();
                app.UseScreenUI();
                app.UseNavigatorAnimation();
                app.UseUGUI();
                app.UseScreenLifecycleEvent();
            }).Build();

        // アプリケーション作成
        var appBuilder = option.ContainerBuilderFactory(navigator.ServiceProvider);
        appBuilder.ServiceCollection.AddSingleton(navigator);
        appBuilder.ServiceCollection.AddNavigationService();
        configure(appBuilder.ServiceCollection);

        return new MVPApplication(appBuilder.Build());
    }
}
```

### 初期化例

```csharp
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
                // サービス登録
                services.AddSingleton<GlobalStore>();
                services.AddTransient<SplashScreen>();
                services.AddTransient<UserProfileScreen>();
            }
        );
        
        // 初期画面で起動
        app.RunAsync<SplashScreen>().Forget();
    }
}
```

## MVPScreen

### 基本構造

`MVPScreen`は画面ロジックとモデル管理を担当する基底クラスです。

**場所**: `Meek.MVP/Runtime/Screen/MVPScreen.cs:34`

```csharp
public abstract class MVPScreen<TModel> : StackScreen
{
    public TModel Model { get; private set; }
    
    protected abstract ValueTask<TModel> CreateModelAsync();
    protected abstract void RegisterEvents(EventHolder eventHolder, TModel model);
    
    // Presenterロードメソッド（2つのオーバーロード）
    protected async Task<TPresenter> LoadPresenterAsync<TPresenter>(IViewHandlerLoader loader, object param = null)
        where TPresenter : class, IPresenter<TModel>
    {
        var viewHandler = await UI.LoadViewHandlerAsync(loader, param) as PrefabViewHandler;
        return viewHandler.Instance.GetComponent<TPresenter>();
    }

    protected async Task<TPresenter> LoadPresenterAsync<TPresenter>(object param = null)
        where TPresenter : class, IPresenter<TModel>
    {
        var factory = AppServices.GetService<IPresenterLoaderFactory>();
        var loader = factory.CreateLoader(this, Model, typeof(TPresenter).Name, param);
        return await LoadPresenterAsync<TPresenter>(loader, param);
    }
}
```

### パラメーター付きScreen

```csharp
public abstract class MVPScreen<TModel, TParam> : MVPScreen<TModel>
{
    private TParam _parameter;

    protected abstract ValueTask<TModel> CreateModelAsync(TParam parameter);

    protected override ValueTask<TModel> CreateModelAsync()
    {
        return CreateModelAsync(_parameter);
    }
    
    protected override async ValueTask StartingImplAsync(StackNavigationContext context)
    {
        // Parameterの代入を行う
        _parameter = context.GetFeatureValue<TParam>(StackNavigationContextFeatureDefine.NextScreenParameter);
        // Parameter代入後にAppBaseScreen<TModel>のStartImplを呼び出す。
        await base.StartingImplAsync(context);
    }
}
```

### 実装例

```csharp
public class UserProfileScreen : MVPScreen<UserProfileModel, UserModel>
{
    protected override async ValueTask<UserProfileModel> CreateModelAsync(UserModel parameter)
    {
        // パラメーターからモデルを生成
        var userRepository = AppServices.GetService<IUserRepository>();
        var userData = await userRepository.GetUserDetailsAsync(parameter.Id);
        return new UserProfileModel(userData);
    }

    protected override void RegisterEvents(EventHolder eventHolder, UserProfileModel model)
    {
        eventHolder.ScreenWillStart(async () =>
        {
            // Presenterをロード
            var presenter = await LoadPresenterAsync<UserProfilePresenter>();
            
            // イベント購読
            presenter.OnEditClick.Subscribe(_ => 
                PushNavigation.PushAsync<UserEditScreen, UserModel>(model.User));
                
            presenter.OnBackClick.Subscribe(_ => 
                PopNavigation.PopAsync());
        });
        
        // 他の画面からのDispatchイベント処理
        eventHolder.Dispatch<UserUpdatedEvent>(userUpdated =>
        {
            model.RefreshUser(userUpdated.User);
            return true; // 処理完了
        });
    }
}
```

### ライフサイクル

**初期化フロー** (`MVPScreen.cs:59`):

1. **StartingImplAsync**: モデル作成とDisposable登録
2. **CreateModelAsync**: 非同期でのモデル初期化
3. **RegisterEvents**: イベント登録とPresenterロード

```csharp
protected override async ValueTask StartingImplAsync(StackNavigationContext context)
{
    Model = await CreateModelAsync();

    // Disposableの自動登録
    if (Model is IAsyncDisposable asyncDisposable) 
        AsyncDisposables.Add(asyncDisposable);
    if (Model is IDisposable disposable) 
        Disposables.Add(disposable);

    await base.StartingImplAsync(context);
}
```

## Presenter

### 基本構造

`Presenter`はViewとModelのバインディングを担当します。

**場所**: `Meek.MVP/Runtime/Presenter/Presenter.cs:8`

```csharp
public abstract class Presenter<TModel> : MonoBehaviour, IPresenter<TModel>, IAsyncDisposable
{
    private readonly List<IDisposable> _disposables = new List<IDisposable>();
    private readonly List<IPresenterEventHandler> _presenterEventHandlers = new List<IPresenterEventHandler>();
    private TModel _model;

    protected abstract IEnumerable<IDisposable> Bind(TModel model);
    protected virtual void OnInit() { }
    protected virtual Task LoadAsync(TModel model) { return Task.CompletedTask; }
    protected virtual Task DisposeAsync() { return Task.CompletedTask; }
    protected virtual void OnSetup(TModel model) { }
    protected virtual void OnDeinit(TModel model) { }
}
```

### ライフサイクル

**実行順序** (`Presenter.cs:14`):

1. **Awake** → **OnInit**: 初期化（モデル設定前）
2. **LoadAsync**: 非同期ロード処理（モデル設定後）  
3. **Setup** → **OnSetup** → **Bind**: セットアップとバインディング
4. **OnDestroy** → **OnDeinit**: 終了処理
5. **DisposeAsync**: 非同期破棄処理（IAsyncDisposable実装）

```csharp
private void Awake()
{
    // IPresenterEventHandlerの収集
    var handlers = this.GetComponents<IPresenterEventHandler>();
    if (handlers != null && handlers.Length > 0) _presenterEventHandlers.AddRange(handlers);

    OnInit();
    foreach (var handler in _presenterEventHandlers) handler.PresenterDidInit(this);
}

Task IPresenter<TModel>.LoadAsync(TModel model)
{
    _model = model;
    return LoadAsync(_model);
}

void IPresenter.Setup()
{
    OnSetup(_model);
    foreach (var handler in _presenterEventHandlers) handler.PresenterDidSetup(this, _model);
    Bind(); // OnSetupの後でBind(_model)を実行
}

private void Bind()
{
    _disposables.AddRange(Bind(_model));
    foreach (var handler in _presenterEventHandlers) handler.PresenterDidBind(this, _model);
}

private void OnDestroy()
{
    OnDeinit(_model);
    foreach (var handler in _presenterEventHandlers) handler.PresenterDidDeinit(this, _model);
    _disposables.DisposeAll();
}

async ValueTask IAsyncDisposable.DisposeAsync()
{
    await DisposeAsync();
}
```

### 実装例

```csharp
public class UserProfilePresenter : Presenter<UserProfileModel>
{
    [SerializeField] private Button _editButton;
    [SerializeField] private Button _backButton;
    [SerializeField] private Text _nameText;
    [SerializeField] private Text _emailText;
    [SerializeField] private Image _avatarImage;
    [SerializeField] private GameObject _loadingSpinner;

    // イベント公開
    public IObservable<Unit> OnEditClick => _editButton.OnClickAsObservable();
    public IObservable<Unit> OnBackClick => _backButton.OnClickAsObservable();

    protected override void OnInit()
    {
        // 初期化処理（モデル設定前）
        _loadingSpinner.SetActive(false);
    }

    protected override async Task LoadAsync(UserProfileModel model)
    {
        // 非同期ロード処理
        if (!string.IsNullOrEmpty(model.User.AvatarUrl))
        {
            var texture = await LoadImageAsync(model.User.AvatarUrl);
            _avatarImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
        }
    }

    protected override void OnSetup(UserProfileModel model)
    {
        // セットアップ処理（バインディング前）
        _editButton.interactable = model.CanEdit;
    }

    protected override IEnumerable<IDisposable> Bind(UserProfileModel model)
    {
        // リアクティブバインディング
        yield return model.User.Name
            .Subscribe(name => _nameText.text = name);
            
        yield return model.User.Email
            .Subscribe(email => _emailText.text = email);
            
        yield return model.IsLoading
            .Subscribe(loading => _loadingSpinner.SetActive(loading));
            
        yield return model.CanEdit
            .Subscribe(canEdit => _editButton.interactable = canEdit);
    }

    protected override async Task DisposeAsync()
    {
        // 非同期破棄処理
        await Task.CompletedTask; // 非同期リソース解放があればここで実行
    }

    protected override void OnDeinit(UserProfileModel model)
    {
        // 同期破棄処理
        if (_avatarImage.sprite != null)
        {
            DestroyImmediate(_avatarImage.sprite.texture);
            DestroyImmediate(_avatarImage.sprite);
        }
    }
}
```

## Model管理

### ReactivePropertyパターン

```csharp
public class UserProfileModel : IDisposable
{
    private readonly CompositeDisposable _disposables = new CompositeDisposable();
    
    // リアクティブプロパティ
    public ReactiveProperty<string> Name { get; }
    public ReactiveProperty<string> Email { get; }
    public ReactiveProperty<bool> IsLoading { get; }
    public ReadOnlyReactiveProperty<bool> CanEdit { get; }
    
    public UserModel User { get; private set; }

    public UserProfileModel(UserModel user)
    {
        User = user;
        
        Name = new ReactiveProperty<string>(user.Name).AddTo(_disposables);
        Email = new ReactiveProperty<string>(user.Email).AddTo(_disposables);
        IsLoading = new ReactiveProperty<bool>(false).AddTo(_disposables);
        
        // 計算プロパティ
        CanEdit = IsLoading.Select(loading => !loading)
            .ToReadOnlyReactiveProperty()
            .AddTo(_disposables);
    }

    public async Task RefreshAsync()
    {
        IsLoading.Value = true;
        try
        {
            // データ更新処理
            var updatedUser = await userRepository.GetUserAsync(User.Id);
            UpdateUser(updatedUser);
        }
        finally
        {
            IsLoading.Value = false;
        }
    }

    private void UpdateUser(UserModel user)
    {
        User = user;
        Name.Value = user.Name;
        Email.Value = user.Email;
    }

    public void Dispose()
    {
        _disposables?.Dispose();
    }
}
```

### ビジネスロジックの分離

```csharp
public class UserProfileModel
{
    private readonly IUserRepository _userRepository;
    private readonly IImageCache _imageCache;

    public UserProfileModel(IUserRepository userRepository, IImageCache imageCache)
    {
        _userRepository = userRepository;
        _imageCache = imageCache;
    }

    public async Task<bool> UpdateProfileAsync(string newName, string newEmail)
    {
        if (string.IsNullOrWhiteSpace(newName) || string.IsNullOrWhiteSpace(newEmail))
            return false;

        IsLoading.Value = true;
        try
        {
            var updateRequest = new UserUpdateRequest
            {
                Id = User.Id,
                Name = newName,
                Email = newEmail
            };

            var updatedUser = await _userRepository.UpdateUserAsync(updateRequest);
            UpdateUser(updatedUser);
            return true;
        }
        catch (Exception ex)
        {
            // エラーハンドリング
            Debug.LogError($"Failed to update user: {ex.Message}");
            return false;
        }
        finally
        {
            IsLoading.Value = false;
        }
    }
}
```

## Presenterローダーシステム

### デフォルトローダー

**場所**: `Meek.MVP/Runtime/Presenter/Loader/PresenterLoaderFactoryFromResources.cs`

```csharp
public class PresenterLoaderFactoryFromResources : IPresenterLoaderFactory
{
    public IViewHandlerLoader CreateLoader<TModel>(IScreen ownerScreen, TModel model, string prefabName, object param)
    {
        return new PresenterLoaderFromResources<TModel>(ownerScreen, model, _prefabViewManager, $"UI/{prefabName}");
    }
}
```

### Presenterの自動ロード

```csharp
// MVPScreen.cs:77
protected async Task<TPresenter> LoadPresenterAsync<TPresenter>(object param = null)
    where TPresenter : class, IPresenter<TModel>
{
    var factory = AppServices.GetService<IPresenterLoaderFactory>();
    // 型名からプレハブ名を自動推定
    var loader = factory.CreateLoader(this, Model, typeof(TPresenter).Name, param);
    return await LoadPresenterAsync<TPresenter>(loader, param);
}
```

### カスタムローダーの作成

```csharp
public class AddressablePresenterLoaderFactory : IPresenterLoaderFactory
{
    public IViewHandlerLoader CreateLoader<TModel>(IScreen ownerScreen, TModel model, string prefabName, object param)
    {
        return new AddressablePresenterLoader<TModel>(ownerScreen, model, $"Presenters/{prefabName}");
    }
}

// DI登録
services.AddSingleton<IPresenterLoaderFactory, AddressablePresenterLoaderFactory>();
```

## 高度なパターン

### 複数Presenterの管理

```csharp
public class ComplexScreen : MVPScreen<ComplexModel>
{
    protected override void RegisterEvents(EventHolder eventHolder, ComplexModel model)
    {
        eventHolder.ScreenWillStart(async () =>
        {
            // メインPresenter
            var mainPresenter = await LoadPresenterAsync<MainPresenter>();
            
            // サブPresenter（条件付きロード）
            if (model.ShowAdvancedOptions)
            {
                var advancedPresenter = await LoadPresenterAsync<AdvancedOptionsPresenter>();
                advancedPresenter.OnOptionChanged.Subscribe(option => 
                    model.UpdateAdvancedOption(option));
            }
            
            // ダイアログPresenter（動的ロード）
            mainPresenter.OnShowDialog.Subscribe(async _ =>
            {
                var dialogPresenter = await LoadPresenterAsync<DialogPresenter>();
                // ダイアログ処理
            });
        });
    }
}
```

### プレゼンターイベントハンドラー

```csharp
public class PresenterEventLogger : MonoBehaviour, IPresenterEventHandler
{
    public void PresenterDidInit(IPresenter presenter)
    {
        Debug.Log($"Presenter initialized: {presenter.GetType().Name}");
    }

    public void PresenterDidBind(IPresenter presenter, object model)
    {
        Debug.Log($"Presenter bound to model: {presenter.GetType().Name}");
    }

    public void PresenterDidSetup(IPresenter presenter, object model)
    {
        Debug.Log($"Presenter setup completed: {presenter.GetType().Name}");
    }

    public void PresenterDidDeinit(IPresenter presenter, object model)
    {
        Debug.Log($"Presenter deinitialized: {presenter.GetType().Name}");
    }
}
```

## ライフサイクル

### 画面ライフサイクルイベント

```csharp
protected override void RegisterEvents(EventHolder eventHolder, Model model)
{
    eventHolder.ScreenWillStart(() => Debug.Log("画面開始前"));
    eventHolder.ScreenDidStart(() => Debug.Log("画面開始後"));
    
    eventHolder.ScreenWillResume(() => Debug.Log("画面復帰前"));
    eventHolder.ScreenDidResume(() => Debug.Log("画面復帰後"));
    
    eventHolder.ScreenWillPause(() => Debug.Log("画面一時停止前"));
    eventHolder.ScreenDidPause(() => Debug.Log("画面一時停止後"));
    
    eventHolder.ScreenWillDestroy(() => Debug.Log("画面破棄前"));
    eventHolder.ScreenDidDestroy(() => Debug.Log("画面破棄後"));
}
```

### ViewWillOpen/DidOpenイベント

```csharp
protected override void RegisterEvents(EventHolder eventHolder, Model model)
{
    eventHolder.ViewWillOpen(() =>
    {
        // アニメーション開始前の処理
        Debug.Log("View animation will start");
    });
    
    eventHolder.ViewDidOpen(() =>
    {
        // アニメーション完了後の処理
        Debug.Log("View animation completed");
        // ユーザー操作を有効化など
    });
}
```

## テスタビリティ

### ユニットテスト例

```csharp
[Test]
public async Task UserProfileModel_UpdateProfile_Success()
{
    // Arrange
    var mockRepository = new Mock<IUserRepository>();
    var user = new UserModel { Id = 1, Name = "John", Email = "john@example.com" };
    var model = new UserProfileModel(mockRepository.Object);
    
    mockRepository.Setup(r => r.UpdateUserAsync(It.IsAny<UserUpdateRequest>()))
              .ReturnsAsync(new UserModel { Id = 1, Name = "Jane", Email = "jane@example.com" });

    // Act
    var result = await model.UpdateProfileAsync("Jane", "jane@example.com");

    // Assert
    Assert.True(result);
    Assert.AreEqual("Jane", model.Name.Value);
    Assert.AreEqual("jane@example.com", model.Email.Value);
}
```

## 次のステップ

- [ミドルウェアシステム](Middleware-System) - カスタムミドルウェアとアニメーション
- [Unity統合](Unity-Integration) - uGUIとVContainerの詳細

---

[[← Navigation System]](Navigation-System) | [[Middleware System →]](Middleware-System)