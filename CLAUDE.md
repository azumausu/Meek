# CLAUDE.md

このファイルは、このリポジトリでコードを扱う際のClaude Code（claude.ai/code）への指針を提供します。

## ドキュメント参照について

### Wikiフォルダの活用
コードを修正・実装する際は、`wiki/`フォルダ内のMarkdownファイルを**必ず参照**してください：

- **`Framework-Overview.md`** - フレームワーク全体のアーキテクチャと設計思想
- **`Navigation-System.md`** - スタックナビゲーションの詳細実装
- **`MVP-Architecture.md`** - MVPパターンとライフサイクル管理
- **`Middleware-System.md`** - ミドルウェアパイプラインの入れ子構造
- **`Unity-Integration.md`** - uGUI/VContainer統合の詳細
- **`API-Reference.md`** - 完全なAPIドキュメント（実装と一致済み）

### 重要な原則
1. **実装前の確認**: 新機能追加や修正前に、該当するwikiドキュメントで設計意図を理解する
2. **アーキテクチャ準拠**: ミドルウェアベース・MVP・スレッドセーフの原則に従う
3. **API一貫性**: fluent API、型安全性、リソース管理パターンを維持する
4. **ドキュメント更新**: 実装変更時は対応するwikiファイルも更新する

### コード修正時の参照手順
```
1. 修正対象の機能領域を特定
2. 対応するwiki/*.mdファイルを読み、設計思想を理解
3. 既存実装との整合性を確認
4. アーキテクチャパターンに従って実装
5. 必要に応じてwikiドキュメントを更新
```

## 開発コマンド

### Unity開発
- **プロジェクトを開く**: Unity Hub → 追加 → このプロジェクトディレクトリを選択
- **ビルド**: Unity Editor → ファイル → ビルド設定 → ビルド（またはCtrl+B）
- **テストを実行**: Unity Editor → ウィンドウ → 一般 → Test Runner
- **デモを実行**: `Assets/Demo/Scenes/Demo.unity`を開いてPlayボタンを押す

### コマンドライン操作
```bash
# ターゲットプラットフォーム用にビルド（Unityのインストールパスに置き換えてください）
/Applications/Unity/Unity.app/Contents/MacOS/Unity -batchmode -quit -projectPath "$(pwd)" -buildTarget <iOS|WebGL|StandaloneOSX> -executeMethod BuildScript.Build

# テストを実行
/Applications/Unity/Unity.app/Contents/MacOS/Unity -batchmode -quit -projectPath "$(pwd)" -runTests -testPlatform editmode

# プロジェクトファイルを生成
/Applications/Unity/Unity.app/Contents/MacOS/Unity -batchmode -quit -projectPath "$(pwd)" -executeMethod UnityEditor.SyncVS.SyncSolution
```

## プロジェクトアーキテクチャ

### コアフレームワーク構造
**Meek**は、スタックベースのナビゲーションを実装したMVPアーキテクチャによるDIベースのUnity UIフレームワークです。

**主要コンポーネント**:
- `Navigator` - ミドルウェアパイプラインを使用するコアナビゲーションオーケストレーター
- `StackNavigationService` - セマフォベースのスレッドセーフティを備えた画面遷移用のメインAPI
- `StackScreenContainer` - 画面スタックを管理（重複する画面タイプは許可されません）
- `MVPApplication` - DIコンテナセットアップを含むアプリケーションエントリポイント

### パッケージ構成
```
Assets/Packages/
├── Meek/                    # コアインターフェースと基底クラス
├── Meek.NavigationStack/    # スタックナビゲーション実装
├── Meek.MVP/               # MVPアーキテクチャサポート
├── Meek.UGUI/              # Unity uGUI統合
└── Meek.VContainer/        # VContainer DI統合
```

### ナビゲーションシステム
**スタックベースナビゲーション**には4つのコア操作があります：
- `PushAsync<T>()` - スタックのトップに画面を追加
- `PopAsync()` - トップの画面を削除
- `InsertScreenBeforeAsync<TBefore, TInsert>()` - 特定の位置に画面を挿入
- `RemoveAsync<T>()` - スタックの中間から画面を削除

**スレッドセーフティ**: すべてのナビゲーション操作は並行性制御のために`SemaphoreSlim`を使用します。

**ミドルウェアパイプライン**: ナビゲーションは設定可能なミドルウェアチェーンを通じて流れます：
1. `ScreenNavigatorEvent` - ナビゲーションライフサイクルイベント
2. `InputLocker` - 遷移中の入力ブロッキング
3. `ScreenUI` - UI作成/破棄
4. `NavigatorAnimation` - 遷移アニメーション
5. `ScreenLifecycleEvent` - 画面ライフサイクルイベント

### MVPアーキテクチャ
**Screen** (`MVPScreen<TModel>`) - 画面ロジックとモデル管理：
- `CreateModelAsync()` - 非同期モデル初期化
- `RegisterEvents()` - ライフサイクルイベントへのサブスクライブ
- `Dispatch()` - 他の画面へのイベント送信

**Presenter** (`Presenter<TModel>`) - ビュー・モデルバインディング：
- `Bind()` - モデルのObservableをビューの更新に接続
- `Resources/UI/`ディレクトリから自動ロード
- ライフサイクル: `OnInit()` → `LoadAsync()` → `OnSetup()` → `Bind()` → `OnDeinit()`

**Model** - ReactivePropertyパターンを使用した状態管理

### アプリケーション初期化
`Main.cs`のエントリポイント：
```csharp
var app = MVPApplication.CreateApp(
    new MVPApplicationOption() {
        ContainerBuilderFactory = x => new VContainerServiceCollection(x),
        InputLocker = inputLocker,
        PrefabViewManager = prefabViewManager,
    },
    services => {
        services.AddSingleton<GlobalStore>();
        services.AddTransient<SplashScreen>();
        // ... 画面を登録
    }
);
app.RunAsync<SplashScreen>();
```

### 主要な依存関係
- **VContainer** - 依存性注入コンテナ
- **UniTask** - 非同期操作
- **UniRx** - モデル状態のためのリアクティブプログラミング
- **Unity uGUI** - UIシステム統合

### 画面ライフサイクルイベント
- `ScreenWillStart/DidStart` - 画面初期化
- `ScreenWillDestroy/DidDestroy` - 画面クリーンアップ
- `ScreenWillResume/DidResume` - 画面がアクティブになる
- `ScreenWillPause/DidPause` - 画面が非アクティブになる
- `ViewWillOpen/DidOpen` - UIアニメーション開始/終了
- `ViewWillClose/DidClose` - UIアニメーション開始/終了

### 開発上の注意
- 画面タイプはナビゲーションスタック内で一意である必要があります
- PresenterプレハブはResources/UI/`ディレクトリにある必要があります
- すべてのナビゲーション操作は非同期処理のために`Task`を返します
- 画面間通信には`Dispatch()`を使用してください
- モデルはクリーンアップのために`IDisposable`を実装すべきです
- 遷移中は入力が自動的にロックされます

## スタックナビゲーション実装詳細

### StackNavigationService
`Assets/Packages/Meek.NavigationStack/Runtime/Serivce/StackNavigationService.cs`

**主要機能**:
- **スレッドセーフティ**: `SemaphoreSlim`を使用して並行アクセスを制御
- **型安全なナビゲーション**: ジェネリクスによる型安全なScreen指定
- **画面間通信**: `Dispatch<T>()`メソッドによるイベント送信

**コア操作の詳細**:

#### PushAsync
```csharp
public async Task PushAsync<TScreen>(PushContext pushContext) where TScreen : IScreen
```
- 新しい画面をスタックのトップに追加
- `NextScreenParameter`でパラメーター渡しが可能
- `IsCrossFade`でクロスフェードアニメーション制御

#### PopAsync
```csharp
public async Task<bool> PopAsync(PopContext popContext)
```
- スタックトップの画面を削除
- `OnlyWhenScreen`で特定画面の時のみPop実行可能
- 戻り値でPop成功/失敗を判定

#### InsertScreenBeforeAsync
```csharp
public Task InsertScreenBeforeAsync<TBeforeScreen, TInsertionScreen>(InsertContext insertionContext)
```
- 指定画面の直前に新しい画面を挿入
- Peek画面への挿入は自動的にPushに変換される

#### RemoveAsync
```csharp
public Task RemoveAsync<TScreen>(RemoveContext removeContext) where TScreen : IScreen
```
- スタック中の指定画面を削除
- Peek画面の削除は自動的にPopに変換される

#### Dispatch機能
```csharp
public void Dispatch<T>(T args)
public Task DispatchAsync<T>(T args)
```
- スタック内の全画面にイベントを送信
- 最初にtrueを返した画面で処理停止

### StackScreenContainer
`Assets/Packages/Meek.NavigationStack/Runtime/ScreenContainer/StackScreenContainer.cs`

**実装詳細**:
- `Stack<IScreen>`でLIFO構造を実装
- Insert/Remove操作では一時的に`_insertOrRemoveCacheStack`を使用
- `IDisposable`実装で適切なリソース管理

### ミドルウェアシステム

#### ScreenUI
`Assets/Packages/Meek.NavigationStack/Runtime/Middleware/ScreenUI/ScreenUI.cs`

**責務**:
- UIコンポーネントの生成/破棄管理
- アニメーション制御（Open/Close/Show/Hide）
- インタラクション制御（LockInteractable）
- 非同期ロード処理

#### NavigatorAnimation
**アニメーションタイプ**:
- `Open` - 画面表示アニメーション
- `Close` - 画面非表示アニメーション
- `Show` - 非アクティブ画面の表示
- `Hide` - アクティブ画面の非表示

#### InputLocker
- 遷移中の入力ブロック
- `LockObject`クラスでロック状態管理

### ナビゲーションコンテキスト
`StackNavigationContext`で遷移情報を管理:
- `NavigatingSourceType` - Push/Pop/Insert/Remove
- `IsCrossFade` - クロスフェード有無
- `SkipAnimation` - アニメーションスキップ
- `Features` - 追加データ格納用Dictionary

## Demo実装例

### アプリケーション構成
`Assets/Demo/Scripts/Main.cs`

**エントリポイント**:
```csharp
public class Main : MonoBehaviour
{
    [SerializeField] private DefaultInputLocker defaultInputLocker;
    [SerializeField] private DefaultPrefabViewManager defaultPrefabViewManager;
    
    public void Start()
    {
        var app = MVPApplication.CreateApp(
            new MVPApplicationOption() {
                ContainerBuilderFactory = x => new VContainerServiceCollection(x),
                InputLocker = defaultInputLocker,
                PrefabViewManager = defaultPrefabViewManager,
            },
            x => {
                x.AddSingleton<GlobalStore>();
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
```

### Screen実装例
`Assets/Demo/Scripts/Screens/SplashScreen.cs`

```csharp
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
            
            presenter.OnClickSignUp.Subscribe(_ => PushNavigation.PushAsync<SignUpScreen>());
            presenter.OnClickLogIn.Subscribe(_ => PushNavigation.PushAsync<LogInScreen>());
        });
    }
}
```

### Presenter実装例
`Assets/Demo/Scripts/Presenters/SplashPresenter.cs`

```csharp
public class SplashPresenter : Presenter<SplashModel>
{
    [SerializeField] private Button _signUpButton;
    [SerializeField] private Button _logInButton;

    public IObservable<Unit> OnClickSignUp => _signUpButton.OnClickAsObservable();
    public IObservable<Unit> OnClickLogIn => _logInButton.OnClickAsObservable();

    protected override IEnumerable<IDisposable> Bind(SplashModel model)
    {
        yield break; // SplashModelは空のため何もバインドしない
    }
}
```

### Model実装例
`Assets/Demo/Scripts/Models/SplashModel.cs`

```csharp
public class SplashModel
{
    // シンプルなスプラッシュ画面のため空実装
}
```

### アプリケーションサービス
`Assets/Demo/Scripts/ApplicationServices/GlobalStore.cs`
- アプリケーション全体で共有される状態管理
- シングルトンとして登録

### リソース管理
**UIプレハブ配置**:
- `Assets/Demo/Resources/UI/` - Presenterプレハブ格納
- 自動ロード機能により命名規則に従って読み込み

**アート素材**:
- `Assets/Demo/Art/` - フォント、UI画像等
- `Assets/Demo/Resources/ProductTextures/` - 商品画像等

### 画面構成例
- **SplashScreen** - 初期画面（SignUp/LogInボタン）
- **SignUpScreen** - ユーザー登録画面
- **LogInScreen** - ログイン画面  
- **TabScreen** - タブ式メイン画面
- **ReviewScreen** - レビュー画面（パラメーター付き遷移）

## Presenterロード先の変更方法

### 現在のロード仕組み
Presenterプレハブは`Resources/UI/`ディレクトリから自動ロードされますが、この設定は以下のファイルで制御されています：

**関連ファイル**:
- `Assets/Packages/Meek.MVP/Runtime/Presenter/Loader/PresenterLoaderFactoryFromResources.cs:19`
- `Assets/Packages/Meek.MVP/Runtime/Presenter/Loader/PresenterLoaderFromResources.cs:32`

### ロード先変更の実装方法

#### 方法1: カスタムPresenterLoaderFactoryの作成（推奨）

独自のローダーファクトリーを作成してDIコンテナで登録：

```csharp
public class CustomPresenterLoaderFactory : IPresenterLoaderFactory
{
    private readonly IPrefabViewManager _prefabViewManager;
    private readonly string _customPath;

    public CustomPresenterLoaderFactory(IPrefabViewManager serviceProvider, string customPath = "CustomUI")
    {
        _prefabViewManager = serviceProvider;
        _customPath = customPath;
    }

    public IViewHandlerLoader CreateLoader<TModel>(IScreen ownerScreen, TModel model, string prefabName, object param)
    {
        return new PresenterLoaderFromResources<TModel>(ownerScreen, model, _prefabViewManager, $"{_customPath}/{prefabName}");
    }
}
```

**アプリケーション初期化での使用**:
```csharp
var app = MVPApplication.CreateApp(
    new MVPApplicationOption() {
        ContainerBuilderFactory = x => new VContainerServiceCollection(x),
        InputLocker = inputLocker,
        PrefabViewManager = prefabViewManager,
    },
    services => {
        // デフォルトのPresenterLoaderFactoryを置き換え
        services.AddSingleton<IPresenterLoaderFactory>(provider => 
            new CustomPresenterLoaderFactory(provider.GetService<IPrefabViewManager>(), "MyCustomUI"));
        
        // 画面登録
        services.AddTransient<SplashScreen>();
        // ...
    }
);
```

この方法により、プレハブを`Resources/MyCustomUI/`ディレクトリから読み込むことができます。

#### 方法2: 既存ファイルの直接修正

`PresenterLoaderFactoryFromResources.cs`の19行目を修正：

```csharp
// 現在
return new PresenterLoaderFromResources<TModel>(ownerScreen, model, _prefabViewManager, $"UI/{prefabName}");

// 修正後
return new PresenterLoaderFromResources<TModel>(ownerScreen, model, _prefabViewManager, $"CustomUI/{prefabName}");
```

**注意**: この方法はフレームワークコードを直接変更するため、アップグレード時に変更が失われる可能性があります。

#### 方法3: Addressable Asset Systemの活用

より高度な実装として、AddressableSystemを使用するカスタムローダーも作成可能：

```csharp
public class AddressablePresenterLoaderFactory : IPresenterLoaderFactory
{
    public IViewHandlerLoader CreateLoader<TModel>(IScreen ownerScreen, TModel model, string prefabName, object param)
    {
        return new AddressablePresenterLoader<TModel>(ownerScreen, model, $"Presenters/{prefabName}");
    }
}
```

### 推奨アプローチ
**方法1のカスタムファクトリー作成**が最も安全で拡張性が高い方法です。フレームワークコードを変更せずに、プロジェクト固有の要件に対応できます。

## ミドルウェアシステム詳細実装

### パイプライン基本構造

#### Navigator クラス
`Assets/Packages/Meek/Runtime/Navigator/Navigator.cs`

```csharp
public class Navigator : INavigator
{
    private readonly NavigationDelegate _application;
    
    public ValueTask NavigateAsync(NavigationContext context)
    {
        return _application(context);
    }
}
```

**NavigationDelegate**: `ValueTask NavigationDelegate(NavigationContext context)`
- ミドルウェアチェーンのコア関数型デリゲート
- 関数型プログラミングによる高階関数合成

#### NavigatorBuilder によるパイプライン構築
```csharp
public INavigator Build()
{
    var stackScreenManager = ServiceProvider.GetService<IScreenContainer>();
    NavigationDelegate app = stackScreenManager.NavigateAsync;
    
    // 逆順でミドルウェアを適用（後に登録されたものが外側になる）
    for (int i = _components.Count - 1; i >= 0; i--)
    {
        app = _components[i].Invoke(app);
    }
    
    return new Navigator(ServiceProvider, app);
}
```

**重要なポイント**:
- ミドルウェアは**逆順**で適用（最後に登録されたものが最初に実行）
- 最終的に`StackScreenContainer.NavigateAsync`が最奥で実行
- ASP.NET Core ミドルウェアパイプラインにインスパイアされた設計

### ミドルウェアインターフェース

#### IMiddleware
```csharp
public interface IMiddleware
{
    ValueTask InvokeAsync(NavigationContext context, NavigationDelegate next);
}
```

#### ミドルウェア登録拡張
```csharp
public static INavigatorBuilder UseMiddleware<T>(this INavigatorBuilder self) where T : IMiddleware
{
    return self.Use(next =>
    {
        return async context =>
        {
            var middleware = self.ServiceProvider.GetService<T>();
            await middleware.InvokeAsync(context, next);
        };
    });
}
```

### デフォルトミドルウェア実行順序

**設定順序** (`MVPApplication.cs`):
```csharp
app.UseDebug();                    // 1. デバッグ情報収集
app.UseScreenNavigatorEvent();     // 2. ナビゲーションイベント制御
app.UseInputLocker();              // 3. 入力ロック
app.UseScreenUI();                 // 4. UI生成・破棄
app.UseNavigatorAnimation();       // 5. アニメーション制御
app.UseUGUI();                     // 6. Unity uGUI統合
app.UseScreenLifecycleEvent();     // 7. ライフサイクルイベント
```

**実際の実行順序** (逆順適用):
1. **ScreenLifecycleEvent** → ライフサイクルイベント処理
2. **UGUI** → Unity uGUI特有の処理
3. **NavigatorAnimation** → 画面遷移アニメーション
4. **ScreenUI** → UI初期化・破棄
5. **InputLocker** → 遷移中の入力制御
6. **ScreenNavigatorEvent** → セマフォによる並行制御
7. **Debug** → デバッグ情報収集
8. **StackScreenContainer** → 最終的な画面スタック操作

### 各ミドルウェア実装詳細

#### ScreenNavigatorEventMiddleware
```csharp
public async ValueTask InvokeAsync(NavigationContext context, NavigationDelegate next)
{
    await _navigationLock.WaitAsync();  // セマフォでスレッドセーフ制御
    
    try
    {
        // 遷移開始イベント
        if (context.FromScreen is IScreenNavigatorEventHandler fromScreenEventHandler)
            fromScreenEventHandler.ScreenWillNavigate(context);

        await next(context);  // 次のミドルウェア実行

        // 遷移完了イベント
        if (context.ToScreen is IScreenNavigatorEventHandler toScreenEventHandler)
            toScreenEventHandler.ScreenDidNavigate(context);
    }
    finally { _navigationLock.Release(); }
}
```

**役割**: 
- `SemaphoreSlim`による並行ナビゲーション制御
- 遷移開始・完了の通知

#### InputLockerMiddleware
```csharp
public async ValueTask InvokeAsync(NavigationContext context, NavigationDelegate next)
{
    using var locker = _inputLocker.LockInput();  // usingによる自動解放
    await next(context);
}
```

**役割**:
- 遷移中の入力をブロック
- `IDisposable`パターンによる確実な解放

#### ScreenUIMiddleware
```csharp
public async ValueTask InvokeAsync(NavigationContext context, NavigationDelegate next)
{
    var stackContext = context.ToStackNavigationContext();
    
    // Push/Insert時の初期化
    if (stackContext.NavigatingSourceType == StackNavigationSourceType.Push)
    {
        stackContext.ToScreen.Initialize(context);
    }
    
    await next(context);
    
    // Pop/Remove時の破棄処理
    if (stackContext.NavigatingSourceType == StackNavigationSourceType.Pop)
    {
        // IAsyncDisposable → IDisposable の順で破棄
        if (context.FromScreen is IAsyncDisposable asyncDisposable)
            await asyncDisposable.DisposeAsync();
        if (context.FromScreen is IDisposable disposable)
            disposable.Dispose();
    }
}
```

**役割**:
- 画面の初期化（Push/Insert時）
- リソース破棄（Pop/Remove時）
- 非同期破棄とスタンダード破棄の両対応

#### NavigatorAnimationMiddleware
```csharp
public async ValueTask InvokeAsync(NavigationContext context, NavigationDelegate next)
{
    await next(context);  // 先にスタック操作を実行
    
    // ViewWillClose/ViewWillOpen イベント発火
    
    // Strategy Pattern によるアニメーション実行
    var strategy = _transitionAnimationModules.LastOrDefault(x => x.IsValid(stackContext));
    if (strategy != null)
    {
        var tcs = new TaskCompletionSource<bool>();
        _coroutineRunner.StartCoroutineWithCallback(strategy.PlayAnimationRoutine(stackContext),
            () => tcs.SetResult(true));
        await tcs.Task;
    }
    
    // ViewDidOpen/ViewDidClose イベント発火
}
```

**役割**:
- Strategy パターンによる遷移タイプ別アニメーション
- ViewWillOpen/DidOpen, ViewWillClose/DidClose イベントの発火
- Unity Coroutine と async/await の統合

#### ScreenLifecycleEventMiddleware
```csharp
private async ValueTask PushAsync(StackNavigationContext context)
{
    // Pause From Screen
    if (context.FromScreen is IScreenLifecycleEventHandler fromScreenEventHandler)
        await fromScreenEventHandler.PausingImplAsync(context);
    
    // Start To Screen
    if (context.ToScreen is IScreenLifecycleEventHandler toScreenEventHandler)
        await toScreenEventHandler.StartingImplAsync(context);
}
```

**役割**:
- 画面ライフサイクルイベントの管理
- Push: Pause(From) → Start(To)
- Pop: Destroy(From) → Resume(To)
- Remove/Insert: 対応する画面の状態変更

### NavigationContext とデータ受け渡し

#### 基本構造
```csharp
public class NavigationContext
{
    public IDictionary<string, object> Features;  // 拡張データ格納
    public IScreen FromScreen;                    // 遷移元画面
    public IScreen ToScreen;                      // 遷移先画面
    public IServiceProvider AppServices;         // DIコンテナ
}
```

#### StackNavigationContext 拡張
```csharp
public class StackNavigationContext : NavigationContext
{
    public StackNavigationSourceType NavigatingSourceType;  // Push/Pop/Insert/Remove
    public bool IsCrossFade;                                // クロスフェード有無
    public bool SkipAnimation;                              // アニメーションスキップ
}
```

### アニメーション戦略システム

#### INavigatorAnimationStrategy
```csharp
public interface INavigatorAnimationStrategy
{
    bool IsValid(StackNavigationContext context);           // 実行条件判定
    IEnumerator PlayAnimationRoutine(StackNavigationContext context);  // アニメーション実行
}
```

#### Push戦略実装例
```csharp
public class PushNavigatorAnimationStrategy : INavigatorAnimationStrategy
{
    bool IsValid(StackNavigationContext context) => 
        context.NavigatingSourceType == StackNavigationSourceType.Push;
    
    IEnumerator PlayAnimationRoutine(StackNavigationContext context)
    {
        if (context.IsCrossFade)
        {
            // 並列実行：Hide(From) + Open(To)
            yield return _coroutineRunner.StartParallelCoroutine(
                fromScreen.UI.HideRoutine(...),
                toScreen.UI.OpenRoutine(...)
            );
        }
        else
        {
            // 順次実行：Hide(From) → Open(To)
            yield return fromScreen.UI.HideRoutine(...);
            yield return toScreen.UI.OpenRoutine(...);
        }
    }
}
```

### カスタムミドルウェアの作成

#### 1. ミドルウェアクラス実装
```csharp
public class CustomMiddleware : IMiddleware
{
    public async ValueTask InvokeAsync(NavigationContext context, NavigationDelegate next)
    {
        // 前処理
        await DoBeforeNavigationAsync(context);
        
        // 次のミドルウェア実行
        await next(context);
        
        // 後処理
        await DoAfterNavigationAsync(context);
    }
}
```

#### 2. DI登録と拡張メソッド
```csharp
public static class CustomMiddlewareExtension
{
    public static IServiceCollection AddCustomMiddleware(this IServiceCollection self)
    {
        self.AddSingleton<CustomMiddleware>();
        return self;
    }
    
    public static INavigatorBuilder UseCustomMiddleware(this INavigatorBuilder app)
    {
        return app.UseMiddleware<CustomMiddleware>();
    }
}
```

#### 3. アプリケーション設定
```csharp
navigatorBuilder
    .ConfigureServices(services => services.AddCustomMiddleware())
    .Configure(app => app.UseCustomMiddleware());
```

### 重要な設計原則

#### スレッドセーフティ
- `ScreenNavigatorEventMiddleware`の`SemaphoreSlim`による排他制御
- 全ナビゲーション操作が単一スレッドで実行される保証

#### リソース管理
- `using`文による確実なリソース解放
- `IAsyncDisposable`/`IDisposable`の適切な処理順序

#### パフォーマンス考慮
- Unity Coroutine と async/await の効率的な統合
- `ObjectPool`を使用したアロケーション最適化

#### 拡張性
- Strategy パターンによる遷移アニメーションのカスタマイズ
- Features Dictionary による任意データの受け渡し
- DIベースの疎結合設計