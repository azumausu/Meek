# APIリファレンス

Meekフレームワークの完全なAPIドキュメントです。

## コアインターフェース

### IScreen

画面の基本インターフェース。

**場所**: `Meek/Runtime/IScreen.cs`

```csharp
public interface IScreen
{
    void Initialize(NavigationContext context);
}
```

**メソッド**:
- `Initialize(NavigationContext context)`: 画面初期化

### INavigator

ナビゲーション実行インターフェース。

**場所**: `Meek/Runtime/Navigator/INavigator.cs`

```csharp
public interface INavigator
{
    IScreenContainer ScreenContainer { get; }
    IServiceProvider ServiceProvider { get; }
    ValueTask NavigateAsync(NavigationContext context);
}
```

**プロパティ**:
- `ScreenContainer`: 画面コンテナ
- `ServiceProvider`: DIコンテナ

**メソッド**:
- `NavigateAsync(NavigationContext context)`: ナビゲーション実行

### IScreenContainer

画面管理インターフェース。

**場所**: `Meek/Runtime/Navigator/IScreenContainer.cs`

```csharp
public interface IScreenContainer
{
    IReadOnlyCollection<IScreen> Screens { get; }
    ValueTask NavigateAsync(NavigationContext context);
}
```

**プロパティ**:
- `Screens`: 管理中の画面一覧

**メソッド**:
- `NavigateAsync(NavigationContext context)`: 画面操作実行

### IMiddleware

ミドルウェアインターフェース。

**場所**: `Meek/Runtime/IMiddleware.cs`

```csharp
public interface IMiddleware
{
    ValueTask InvokeAsync(NavigationContext context, NavigationDelegate next);
}
```

**メソッド**:
- `InvokeAsync(NavigationContext, NavigationDelegate)`: ミドルウェア処理

## ナビゲーションAPI

### StackNavigationService

**場所**: `Meek.NavigationStack/Runtime/Serivce/StackNavigationService.cs:10`

#### PushAsync

```csharp
public Task PushAsync<TScreen>(PushContext pushContext) where TScreen : IScreen
public async Task PushAsync(Type screenClassType, PushContext pushContext)
```

**パラメーター**:
- `TScreen` / `screenClassType`: 追加する画面の型
- `pushContext`: Push操作のコンテキスト

**戻り値**: `Task` - 非同期操作

#### PopAsync

```csharp
public async Task<bool> PopAsync(PopContext popContext)
```

**パラメーター**:
- `popContext`: Pop操作のコンテキスト

**戻り値**: `Task<bool>` - Pop成功時true

#### InsertScreenBeforeAsync

```csharp
public Task InsertScreenBeforeAsync<TBeforeScreen, TInsertionScreen>(InsertContext insertionContext)
    where TBeforeScreen : IScreen where TInsertionScreen : IScreen
public Task InsertScreenBeforeAsync(Type beforeScreenClassType, Type insertionScreenClassType, InsertContext insertionContext)
public async Task InsertScreenBeforeAsync(IScreen beforeScreen, Type insertionScreenClassType, InsertContext insertionContext)
```

**パラメーター**:
- `TBeforeScreen` / `beforeScreenClassType` / `beforeScreen`: 挿入位置の基準画面
- `TInsertionScreen` / `insertionScreenClassType`: 挿入する画面の型
- `insertionContext`: Insert操作のコンテキスト

#### RemoveAsync

```csharp
public Task RemoveAsync<TScreen>(RemoveContext removeContext) where TScreen : IScreen
public Task RemoveAsync(Type removeScreenClassType, RemoveContext removeContext)
public async Task RemoveAsync(IScreen removeScreen, RemoveContext removeContext)
```

**パラメーター**:
- `TScreen` / `removeScreenClassType` / `removeScreen`: 削除する画面
- `removeContext`: Remove操作のコンテキスト

#### Dispatch

```csharp
public void Dispatch<T>(T args)
public void Dispatch(string eventName, object parameter)
public Task DispatchAsync<T>(T args)
public Task DispatchAsync(string eventName, object parameter)
```

**パラメーター**:
- `T` / `eventName`: イベント型または名前
- `args` / `parameter`: イベントパラメーター

**戻り値**: 
- 同期版：`void` （戻り値なし）
- 非同期版：`Task` （戻り値なし）

**注意**: 内部的には最初にtrueを返した画面で処理停止しますが、publicメソッドには戻り値がありません。

### 操作コンテキスト

#### PushContext

```csharp
public class PushContext
{
    public object NextScreenParameter = null;
    public bool IsCrossFade = false;
    public bool SkipAnimation = false;
}
```

#### PopContext

```csharp
public class PopContext
{
    public bool IsCrossFade = false;
    public bool SkipAnimation = false;
    public IScreen? OnlyWhenScreen = null;
    public bool UseOnlyWhenScreen => OnlyWhenScreen != null;  // computed property
}
```

**注意**: `UseOnlyWhenScreen` は計算プロパティで、`OnlyWhenScreen` が null でない場合に true を返します。

#### InsertContext

```csharp
public class InsertContext
{
    public object NextScreenParameter = null;
    public bool IsCrossFade = false;
    public bool SkipAnimation = true;  // デフォルトでアニメーションスキップ
}
```

#### RemoveContext

```csharp
public class RemoveContext
{
    public bool IsCrossFade = false;
    public bool SkipAnimation = true;  // デフォルトでアニメーションスキップ
}
```

## MVPアーキテクチャAPI

### MVPScreen

**場所**: `Meek.MVP/Runtime/Screen/MVPScreen.cs:34`

#### 基本クラス

```csharp
public abstract class MVPScreen<TModel> : StackScreen
{
    public TModel Model { get; private set; }
    
    protected abstract ValueTask<TModel> CreateModelAsync();
    protected abstract void RegisterEvents(EventHolder eventHolder, TModel model);
    protected async Task<TPresenter> LoadPresenterAsync<TPresenter>(object param = null)
        where TPresenter : class, IPresenter<TModel>
}
```

#### パラメーター付きクラス

```csharp
public abstract class MVPScreen<TModel, TParam> : MVPScreen<TModel>
{
    protected abstract ValueTask<TModel> CreateModelAsync(TParam parameter);
}
```

**メソッド**:
- `CreateModelAsync()`: モデル作成（必須実装）
- `RegisterEvents()`: イベント登録（必須実装）
- `LoadPresenterAsync<T>()`: Presenterロード

### Presenter

**場所**: `Meek.MVP/Runtime/Presenter/Presenter.cs:8`

```csharp
public abstract class Presenter<TModel> : MonoBehaviour, IPresenter<TModel>, IAsyncDisposable
{
    protected abstract IEnumerable<IDisposable> Bind(TModel model);
    protected virtual void OnInit() { }
    protected virtual Task LoadAsync(TModel model) { return Task.CompletedTask; }
    protected virtual void OnSetup(TModel model) { }
    protected virtual void OnDeinit(TModel model) { }
}
```

**メソッド**:
- `Bind(TModel model)`: モデルバインディング（必須実装）
- `OnInit()`: 初期化処理（Awake時）
- `LoadAsync(TModel model)`: 非同期ロード処理（モデル設定後）
- `OnSetup(TModel model)`: セットアップ処理（Bind前）
- `OnDeinit(TModel model)`: 終了処理（OnDestroy時）
- `DisposeAsync()`: 非同期リソース解放（virtual、オーバーライド可能）

**ライフサイクル順序**:
1. `Awake` → `OnInit()`
2. `LoadAsync(TModel)`
3. `OnSetup(TModel)` → `Bind(TModel)`
4. `OnDestroy` → `OnDeinit(TModel)`
5. `DisposeAsync()` (IAsyncDisposable実装)

### IPresenter

```csharp
public interface IPresenter
{
    void Setup();
}

public interface IPresenter<in TModel> : IPresenter
{
    Task LoadAsync(TModel model);
}
```

### MVPApplication

**場所**: `Meek.MVP/Runtime/MVPApplication.cs:10`

```csharp
public class MVPApplication : IDisposable
{
    public IServiceProvider AppServices { get; }
    
    public Task RunAsync<TBootScreen>() where TBootScreen : IScreen
    public void Dispose()
    
    public static MVPApplication CreateApp(MVPApplicationOption option, Action<IServiceCollection> configure)
}
```

#### MVPApplicationOption

```csharp
public class MVPApplicationOption
{
    public Func<IServiceProvider, IContainerBuilder> ContainerBuilderFactory { get; set; }
    public IInputLocker InputLocker { get; set; }
    public IPrefabViewManager PrefabViewManager { get; set; }
    public IServiceProvider Parent { get; set; }
}
```

## ライフサイクルイベント

### 画面ライフサイクル

#### IScreenLifecycleEventHandler

```csharp
public interface IScreenLifecycleEventHandler
{
    ValueTask StartingImplAsync(StackNavigationContext context);
    ValueTask DestroyingImplAsync(StackNavigationContext context);
    ValueTask ResumingImplAsync(StackNavigationContext context);
    ValueTask PausingImplAsync(StackNavigationContext context);
}
```

#### ScreenLifecycleEvent

```csharp
public static class ScreenLifecycleEvent
{
    public const string ScreenWillStart = "ScreenWillStart";
    public const string ScreenDidStart = "ScreenDidStart";
    public const string ScreenWillDestroy = "ScreenWillDestroy";
    public const string ScreenDidDestroy = "ScreenDidDestroy";
    public const string ScreenWillResume = "ScreenWillResume";
    public const string ScreenDidResume = "ScreenDidResume";
    public const string ScreenWillPause = "ScreenWillPause";
    public const string ScreenDidPause = "ScreenDidPause";
}
```

### Viewライフサイクル

#### NavigatorAnimationScreenEvent

```csharp
public static class NavigatorAnimationScreenEvent
{
    public const string ViewWillOpen = "ViewWillOpen";
    public const string ViewDidOpen = "ViewDidOpen";
    public const string ViewWillClose = "ViewWillClose";
    public const string ViewDidClose = "ViewDidClose";
}
```

### EventHolder拡張メソッド

```csharp
public static class ScreenLifecycleEventHolderExtension
{
    public static void ScreenWillStart(this EventHolder self, Action action)
    public static void ScreenWillStart(this EventHolder self, Func<Task> asyncAction)
    public static void ScreenDidStart(this EventHolder self, Action action)
    public static void ScreenDidStart(this EventHolder self, Func<Task> asyncAction)
    
    public static void ScreenWillDestroy(this EventHolder self, Action action)
    public static void ScreenWillDestroy(this EventHolder self, Func<Task> asyncAction)
    public static void ScreenDidDestroy(this EventHolder self, Action action)
    public static void ScreenDidDestroy(this EventHolder self, Func<Task> asyncAction)
    
    public static void ScreenWillResume(this EventHolder self, Action action)
    public static void ScreenWillResume(this EventHolder self, Func<Task> asyncAction)
    public static void ScreenDidResume(this EventHolder self, Action action)
    public static void ScreenDidResume(this EventHolder self, Func<Task> asyncAction)
    
    public static void ScreenWillPause(this EventHolder self, Action action)
    public static void ScreenWillPause(this EventHolder self, Func<Task> asyncAction)
    public static void ScreenDidPause(this EventHolder self, Action action)
    public static void ScreenDidPause(this EventHolder self, Func<Task> asyncAction)
}
```

```csharp
public static class ScreenViewEventHolderExtension
{
    public static void ViewWillOpen(this EventHolder self, Action action)
    public static void ViewWillOpen(this EventHolder self, Func<Task> asyncAction)
    public static void ViewDidOpen(this EventHolder self, Action action)
    public static void ViewDidOpen(this EventHolder self, Func<Task> asyncAction)
    
    public static void ViewWillClose(this EventHolder self, Action action)
    public static void ViewWillClose(this EventHolder self, Func<Task> asyncAction)
    public static void ViewDidClose(this EventHolder self, Action action)
    public static void ViewDidClose(this EventHolder self, Func<Task> asyncAction)
}
```

## Unity統合API

### UGUI統合

#### IInputLocker

```csharp
public interface IInputLocker
{
    IDisposable LockInput();
    bool IsInputLocking { get; }
}
```

**メソッド**:
- `LockInput()`: 入力をロックし、Disposeで解放

**プロパティ**:
- `IsInputLocking`: 現在の入力ロック状態

#### IPrefabViewManager

```csharp
public interface IPrefabViewManager
{
    Transform GetRootNode(IScreen ownerScreen, [CanBeNull] object param = null);
    void SortOrderInHierarchy(NavigationContext navigationContext);
}
```

**メソッド**:
- `GetRootNode()`: UI要素の親ノードを取得
- `SortOrderInHierarchy()`: UI要素の描画順序を管理

**注意**: プレハブのロード機能は `PresenterLoaderFromResources` クラスで実装されています。

#### INavigatorAnimation

```csharp
public interface INavigatorAnimation
{
    NavigatorAnimationType NavigatorAnimationType { get; }
    float Length { get; }
    string FromScreenName { get; }
    string ToScreenName { get; }
    void Evaluate(float t);
    void Play(Action onComplete = null);
    void Stop();
}
```

**プロパティ**:
- `NavigatorAnimationType`: アニメーションの種類
- `Length`: アニメーションの長さ（秒）
- `FromScreenName`: 遷移元画面名（フィルタリング用）
- `ToScreenName`: 遷移先画面名（フィルタリング用）

**メソッド**:
- `Evaluate(float t)`: 指定された時間でアニメーションを評価
- `Play(Action onComplete)`: アニメーションを再生
- `Stop()`: アニメーションを停止

#### NavigatorAnimationType

```csharp
public enum NavigatorAnimationType
{
    Open,   // 画面表示アニメーション
    Close,  // 画面非表示アニメーション
    Show,   // 非アクティブ画面の表示
    Hide    // アクティブ画面の非表示
}
```

### VContainer統合

#### IServiceCollection

```csharp
public interface IServiceCollection
{
    IServiceCollection AddSingleton<TService>();
    IServiceCollection AddSingleton<TService, TImplementation>() where TImplementation : class, TService;
    IServiceCollection AddSingleton<TService>(TService instance);
    IServiceCollection AddSingleton<TService>(Func<IServiceProvider, TService> factory);
    
    IServiceCollection AddTransient<TService>();
    IServiceCollection AddTransient<TService, TImplementation>() where TImplementation : class, TService;
    IServiceCollection AddTransient<TService>(Func<IServiceProvider, TService> factory);
    
    IServiceCollection AddScoped<TService>();
    IServiceCollection AddScoped<TService, TImplementation>() where TImplementation : class, TService;
    IServiceCollection AddScoped<TService>(Func<IServiceProvider, TService> factory);
}
```

#### IServiceProvider

標準の .NET `System.IServiceProvider` を使用し、Meek では拡張メソッドでジェネリック対応を提供：

```csharp
// 標準インターフェース
public interface IServiceProvider
{
    object GetService(Type serviceType);
}

// Meek拡張メソッド
public static class ServiceProviderExtensions
{
    public static T GetService<T>(this IServiceProvider serviceProvider);
}
```

## 設計原則

### スレッドセーフティ

**SemaphoreSlim による排他制御**:
```csharp
private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

public async Task NavigateAsync()
{
    await _semaphoreSlim.WaitAsync();
    try
    {
        // ナビゲーション処理
    }
    finally
    {
        _semaphoreSlim.Release();
    }
}
```

### リソース管理

**IDisposable パターン**:
```csharp
public class ResourceManagedClass : IDisposable
{
    private readonly CompositeDisposable _disposables = new();
    
    public void AddDisposable(IDisposable disposable)
    {
        _disposables.Add(disposable);
    }
    
    public void Dispose()
    {
        _disposables?.Dispose();
    }
}
```

### 非同期処理

**async/await パターン**:
```csharp
public async ValueTask ProcessAsync()
{
    // 非同期初期化
    await InitializeAsync();
    
    try
    {
        // メイン処理
        await MainProcessAsync();
    }
    finally
    {
        // クリーンアップ
        await CleanupAsync();
    }
}
```

## 拡張メソッド

### NavigationContext拡張

```csharp
public static class NavigationContextExtension
{
    public static StackNavigationContext ToStackNavigationContext(this NavigationContext self)
    {
        return self as StackNavigationContext ?? throw new InvalidCastException();
    }
    
    public static T GetFeatureValue<T>(this NavigationContext self, string key)
    {
        if (self.Features.TryGetValue(key, out var value) && value is T typedValue)
        {
            return typedValue;
        }
        return default(T);
    }
}
```

### Navigation API使用例

```csharp
// 基本的な画面遷移
await PushNavigation.PushAsync<UserProfileScreen>();

// パラメーター付き遷移（fluent API）
await PushNavigation
    .NextScreenParameter(userData)
    .PushAsync<UserDetailScreen>();

// オプション付き遷移
await PushNavigation
    .IsCrossFade(true)
    .SkipAnimation(false)
    .PushAsync<ModalScreen>();

// 条件付きPop
await PopNavigation
    .OnlyWhen(currentScreen)
    .PopAsync();

// 画面挿入
await InsertNavigation
    .NextScreenParameter(parameter)
    .InsertScreenBeforeAsync<TargetScreen, NewScreen>();
```

### IDisposable拡張

```csharp
public static class IDisposableExtension
{
    public static void DisposeAll(this IEnumerable<IDisposable> disposables)
    {
        foreach (var disposable in disposables)
        {
            disposable?.Dispose();
        }
    }
    
    public static T AddTo<T>(this T disposable, ICollection<IDisposable> container) where T : IDisposable
    {
        container.Add(disposable);
        return disposable;
    }
}
```

## エラーハンドリング

### 標準例外

```csharp
// 画面が見つからない場合
public class ScreenNotFoundException : Exception
{
    public ScreenNotFoundException(string screenName) 
        : base($"Screen not found: {screenName}") { }
}

// ナビゲーション操作が無効な場合
public class InvalidNavigationOperationException : Exception
{
    public InvalidNavigationOperationException(string operation, string reason) 
        : base($"Invalid navigation operation '{operation}': {reason}") { }
}

// プレハブロードに失敗した場合
public class PrefabLoadException : Exception
{
    public PrefabLoadException(string path, Exception innerException) 
        : base($"Failed to load prefab at path: {path}", innerException) { }
}
```

### エラーハンドリングパターン

```csharp
public async Task<bool> SafeNavigateAsync<TScreen>() where TScreen : IScreen
{
    try
    {
        await PushNavigation.PushAsync<TScreen>();
        return true;
    }
    catch (ScreenNotFoundException ex)
    {
        Debug.LogError($"Screen not found: {ex.Message}");
        return false;
    }
    catch (InvalidNavigationOperationException ex)
    {
        Debug.LogWarning($"Navigation blocked: {ex.Message}");
        return false;
    }
    catch (Exception ex)
    {
        Debug.LogError($"Unexpected navigation error: {ex}");
        return false;
    }
}

// パラメーター付きナビゲーションのエラーハンドリング例
public async Task<bool> SafeNavigateWithParameterAsync<TScreen, TParam>(TParam parameter) 
    where TScreen : IScreen
{
    try
    {
        await PushNavigation
            .NextScreenParameter(parameter)
            .PushAsync<TScreen>();
        return true;
    }
    catch (Exception ex)
    {
        Debug.LogError($"Navigation with parameter failed: {ex}");
        return false;
    }
}
```

## パフォーマンス最適化

### ObjectPool パターン

```csharp
// StackNavigationService での使用例
DictionaryPool<string, object>.Get(out var features);
try
{
    // features 使用
}
finally
{
    DictionaryPool<string, object>.Release(features);
}
```

### メモリ効率化

```csharp
public class MemoryEfficientScreen : MVPScreen<SomeModel>
{
    protected override void RegisterEvents(EventHolder eventHolder, SomeModel model)
    {
        eventHolder.ScreenWillDestroy(() =>
        {
            // 明示的なメモリ解放
            Resources.UnloadUnusedAssets();
            GC.Collect();
        });
    }
}
```

---

[[← Unity Integration]](Unity-Integration) | [[Home]](Home)