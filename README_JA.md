# Meek
Meekは、DIベースで実装されたUnityのユーザーインターフェイスを構築するためのライブラリです。  
主な機能としては、画面遷移や画面ライフサイクルの管理機能を提供しています。  
また、MVPアーキテクチャでの実装を容易にするためのツールも提供しています。  

[MeekDemo](https://user-images.githubusercontent.com/19426596/232242080-f2eac6e7-e1ae-48c3-9816-8aebae1f951b.mov)

デモの中で使用している画像はフリーのコンテンツです。    
著作権に関する情報は以下のウェブサイトをご確認ください。  
[Nucleus UI](https://www.nucleus-ui.com/)

# 要件
- Unity 2021.3 (LTS) or newer
- uGUI
- [VContainer](https://github.com/hadashiA/VContainer)



# インストール
以下をPackages/manifest.jsonに追加する必要があります。  

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

MVPパターンでの実装を行う場合は、[UniRx](https://github.com/neuecc/UniRx) を使用することをお勧めします。

# 基本的な使い方
## エントリポイント
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
            // アプリケーションを作成します。
            var app = MVPApplication.CreateRootApp(
                // アプリケーションの初期化時にオプションを指定します。
                new MVPRootApplicationOption()
                {
                    // DIコンテナを作成するためのファクトリメソッドです。
                    ContainerBuilderFactory = x => new VContainerServiceCollection(x),
                    // 画面遷移時に入力をロックするためのクラスです。
                    InputLocker = _inputLocker,
                    // 画面遷移時に表示するプレハブを管理するクラスです。
                    PrefabViewManager = _prefabViewManager,
                },
                // 使用する画面のクラスや機能を登録します。
                x =>
                {
                    // App Services
                    x.AddSingleton<GlobalStore>();
                    
                    // Screen
                    x.AddTransient<SplashScreen>();
                    x.AddTransient<SignUpScreen>();
                    x.AddTransient<LogInScreen>();
                    x.AddTransient<TabScreen>();
                    x.AddTransient<ReviewScreen>();
                }
            );
            // アプリケーションを起動する最初のScreenを指定します。
            app.RunAsync<SplashScreen>().Forget();
        }
    }
}
```
`MVPApplication.CreateRootApp`を呼び出すことで、アプリケーションを作成します。
アプリケーションを作成した後に、`app.RunAsync<TScreen>`を呼び出すことで、最初の画面を表示します。

### InputLocker
以下のInterfaceを実装する必要があります。
```csharp
public interface IInputLocker
{
    // Inputをロック機能を実装してください。
    IDisposable LockInput();

    // Inputをロックしているかどうかを返してください
    public bool IsInputLocking { get; }
}
```
Demoでは、以下のように実装しています。
```csharp
public class InputLocker : MonoBehaviour, IInputLocker
{
    // Unity uGUIのImageコンポーネントです。
    [SerializeField] private Image _inputBlocker;
    
    public IDisposable LockInput()
    {
        _inputBlocker.enabled = true;
        return new Disposer(() => _inputBlocker.enabled = false);
    }
    
    public bool IsInputLocking => _inputBlocker.enabled;
}
```
### PrefabViewManager
以下のInterfaceを実装する必要があります。
```csharp
public interface IPrefabViewManager
{
    // 管理しているPrefabの描画順を変更する処理を実装してください。
    // 画面遷移時に呼び出されます。
    void SortOrderInHierarchy(NavigationContext context);
    
    // 作成されたPrefabをHierarchyに追加する処理を実装してください。
    void AddInHierarchy(PrefabViewHandler handler);
}
```

Demoでは、以下のように実装しています。
```csharp
public class PrefabViewManager : MonoBehaviour, IPrefabViewManager
{
    [SerializeField] private Transform _rootNode;
    
    void IPrefabViewManager.AddInHierarchy(PrefabViewHandler handler)
    {
        // 配置するNodeに合わせてLayerを設定します。
        handler.RootNode.gameObject.SetLayerRecursively(_rootNode.gameObject.layer);
        handler.RootNode.SetParent(_rootNode);
    }

    void IPrefabViewManager.SortOrderInHierarchy(NavigationContext navigationContext)
    {
        // 新しく作成されたものが最前面に来るように、Hierarchyの順番を変更します。
        var navigationService = navigationContext.AppServices.GetService<StackNavigationService>();
        var uis = navigationService.ScreenContainer.Screens.OfType<StackScreen>().Select(x => x.UI);
        foreach (var ui in uis)
        {
            foreach (var prefabView in ui.ViewHandlers.Reverse().OfType<PrefabViewHandler>())
                prefabView.RootNode.SetAsFirstSibling();
        }
    }
}
```


## MVP(Model-View-Presenter)
MeekのMVPアーキテクチャは、以下のような構成になっています。  
![MVP](Docs/Assets/MVP.png)  
通常のMVPアーキテクチャとの違いは、PresenterではなくScreenがModelを更新するところです。
また、PresenterをロードするとModelクラスが自動でPresenterにDIされます。
### Screen
Screenは画面を表すクラスです。
MVPアーキテクチャで実装する場合は、以下の役割を持ちます。
- Modelの初期化
- Modelの更新
- Presenterの初期化
- Presenterのイベントを購読
- 他Screenへの遷移

```csharp
public class FavoritesScreen : MVPScreen<FavoritesModel>
{
    // Modelを作成します。
    protected override async ValueTask<FavoritesModel> CreateModelAsync()
    {
        // DIコンテナに登録したServiceは、
        // コンストラクタインジェクションかベースクラスが保持しているAppServicesから取得できます。
        var globalStore = AppServices.GetService<GlobalStore>();
        return await Task.FromResult(new FavoritesModel(globalStore));
    }

    // Screenのライフサイクルイベントを購読します。
    protected override void RegisterEvents(EventHolder eventHolder, FavoritesModel model)
    {
        // Screenがアクティブになる前に呼び出されます。
        eventHolder.ScreenWillStart(async () =>
        {
            // Presenterをロードします。
            var presenter = await LoadPresenterAsync<FavoritesPresenter>();
            
            // Presenterが公開しているイベントを購読する場合は以下に記述します。
        });
    }
}
```
Screenは、`MVPScreen<TModel>`を継承します。また、`CreateModelAsync`と`RegisterEvents`を実装する必要があります。  
`CreateModelAsync`は、Modelを作成します。 Serverとの通信などの非同期処理を挟めるようにasync/awaitを使用しています。  
`RegisterEvents`は、Screenのライフサイクルイベントを購読し必要な処理を実装します。  
サンプルでは、`ScreenWillStart`（Screenがアクティブになる前）を使用してPresenterをロードしています。

### Model
Modelは画面の状態を表すクラスです。
1つのScreenにつき1つのModelを持ちます。

```csharp
public class LogInModel
{
    // Modelの状態をReactivePropertyで定義します。
    private ReactiveProperty<string> _email = new();
    private ReactiveProperty<string> _password = new();
    
    public IReadOnlyReactiveProperty<string> Email => _email;
    public IReadOnlyReactiveProperty<string> Password => _password;
    
    // Modelの状態を更新する関数を実装します。
    public void UpdateEmail(string value)
    {
        _email.Value = value;
    }
    
    public void UpdatePassword(string value)
    {
        _password.Value = value;
    }

    public async Task LogInAsync()
    {
        // Login 処理をここに実装します。
    }
}
```
### Presenter
Presenterは画面の表示を表すクラスです。  
Demoでは、`Resources/UI`ディレクトリにPrefabを配置しロードしています。 
![PrefabResourcesFolder](Docs/Assets/PrefabResourcesFolder.png)

> **ヒント**  
> デフォルトでは`Resources/UI`をディレクトリからPrefabをロードしますが、
> アプリケーションを作成する時に、`IPresenterLoaderFactory`を渡すことでカスタムすることが可能です。

Prefabは下の画像のように、PrefabのルートにPresenterをアタッチする必要があります。
![PrefabPresenter](Docs/Assets/PrefabPresenter.png)

```csharp
public class LogInPresenter : Presenter<LogInModel>
{
    // ViewをSerializeFieldで定義します。
    [SerializeField] private InputFieldView _emailInputField;
    [SerializeField] private InputFieldView _passwordInputField;

    [SerializeField] private Button _backButton;
    [SerializeField] private Button _logInButton;

    // ViewのイベントをIObservableで定義します。
    public IObservable<Unit> OnClickBack => _backButton.OnClickAsObservable();
    public IObservable<Unit> OnClickLogIn => _logInButton.OnClickAsObservable();
    
    public IObservable<string> OnEndEditEmail => _emailInputField.OnEndEdit;
    public IObservable<string> OnEndEditPassword => _passwordInputField.OnEndEdit;

    // Modelのイベントを購読して、Viewの状態を更新する関数を呼び出します。
    protected override IEnumerable<IDisposable> Bind(LogInModel model)
    {
        yield return model.Email.Subscribe(x => _emailInputField.UpdateView(x));
        yield return model.Password.Subscribe(x => _passwordInputField.UpdateView(x));
    }
}
```
`Bind`でModelのイベントを購読して、Viewの状態を更新する関数を呼び出します。  
また、`Bind`以外に以下の関数を実装することができます。  
```csharp
// Presenterがインスタンス化された時に呼び出されます。
protected virtual void OnInit() { }

// Presenterがロードされた時に呼び出されます。
// Presenterのロードに合わせて、UnityのSceneAssetや追加のPrefabをロードすることができます。
protected virtual Task LoadAsync(TModel model) { return Task.CompletedTask; }

// Bindの直前に呼び出されます。
// 主にPresenterが持つViewの初期化処理を行ます。
protected virtual void OnSetup(TModel model) { }

// Presenterが破棄された時に呼び出されます。
protected virtual void OnDeinit(TModel model) { }
```

その他の実装のポイント
- PresenterでModelの状態を書き換えない  
=> 複雑になるとScreenとPresenterの双方でModelを書き換えることで可読性が下がります
- Presenterには極力ロジックは含めない  
=> PresenterはViewとModelをBindingする役割に留めた方が可読性が上がります
- Presenterには独自の関数を定義しない  
=> 理想的にはPresenterはロジックを持たないので、関数を定義する必要がないはずです
- Viewは純粋関数で構成する  
=> SSOT(Single Source of Truth)に従って、必ずModelが持つ状態が唯一の情報源となるように実装しましょう。

> **ヒント**  
> InGameなど、複雑なViewを実装する必要がある場合はPresenterの配下に
> 別のアーキテクチャを採用することをお勧めします。

## Navigation
MeekのStackNavigatorは[MAUI](https://learn.microsoft.com/en-us/dotnet/maui/user-interface/pages/navigationpage?view=net-maui-7.0)のNavigation機能を参考にしています。  
基本機能は、以下の4つがあります。
- Push
- Pop
- InsertScreenBefore
- Remove

これらの4つの機能を組み合わせて、様々な画面遷移を実装します。  
また、Navigation機能はScreenクラス内で呼び出します。
> **注意**  
> StackNavigatorは、同じ型のScreenを同時に複数持つことができません。

> **ヒント**  
> StackNavigatorは、[Middlewareパターン](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-7.0)を参考に実装しているので、使用するMiddlewareを変更して機能をカスタムすることも可能です。

### Push
Pushは、現在のScreenの上に新しいScreenを追加します。  
遷移先のScreenは、Generic引数で指定します。
```csharp
PushNavigation.PushAsync<TabScreen>();
```
非同期関数なので、Push処理が終了するまで待機することも可能です。
また、以下のように`UpdateNextScreenParameter`を呼び出すことで次のScreenに状態を渡すことができます。
```csharp
PushNavigation
    // ここで状態を渡します。
    .UpdateNextScreenParameter(new ReviewScreenParameter(){ ProductId = id, })
    .PushAsync<ReviewScreen>().Forget();
```
上の例では、ReviewScreenに対してProductIdを渡しています。  
受け取る側のScreenでは以下のように実装します。
```csharp
// Generic引数の型にParameterを指定します。
public class ReviewScreen : MVPScreen<ReviewModel, ReviewScreenParameter>
{
    private readonly StackNavigationService _stackNavigationService;
    
    public ReviewScreen(StackNavigationService stackNavigationService)
    {
        _stackNavigationService = stackNavigationService;
    }
    
    // CreateModelAsync関数でParameterを受け取り、Modelを生成します。
    protected override async ValueTask<ReviewModel> CreateModelAsync(ReviewScreenParameter parameter)
    {
        return await Task.FromResult(new ReviewModel(parameter.ProductId));
    }
}
```

### Pop
Popは、現在のScreenを削除します。
```csharp
PopNavigation.PopAsync();
```

### InsertScreenBefore
ScreenStackの途中に新しいScreenを追加します。
Genericの第一引数で指定したScreenの上に、第二引数で指定したScreenを追加します。
```csharp
InsertNavigation.InsertScreenBeforeAsync<SplashScreen, HomeScreen>();
```

### Remove
ScreenStackの途中にあるScreenを取り除きます。  
取り除きたいScreenは、Generic引数で指定します。
```csharp
RemoveNavigation.RemoveAsync<SignUpScreen>();
```

### StackNavigatorの処理フロー（発展）
StackNavigatorは、以下のような処理フローで動作します。
![StackNavigatorProcess](Docs/Assets/StackNavigatorProcess.png)
```csharp
// NavigatorBuilderを作成します。
var stackNavigator = new NavigatorBuilder(navigatorBuilderOption =>
{
    // Optionパターンで設定を行います。
    navigatorBuilderOption.ContainerBuilder = option.ContainerBuilder;
    navigatorBuilderOption.ScreenContainer = typeof(StackScreenContainer);
}).ConfigureServices(serviceCollection =>
{
    // 必要なServiceをDIコンテナに登録します。
    serviceCollection.AddScreenNavigatorEvent();
    serviceCollection.AddInputLocker(x => { x.InputLocker = option.InputLocker; });
    serviceCollection.AddScreenUI();
    serviceCollection.AddNavigatorAnimation(
        x =>
        {
            // 使用する遷移アニメーションのロジックを登録します。
            x.Strategies.Add<PushNavigatorAnimationStrategy>();
            x.Strategies.Add<PopNavigatorAnimationStrategy>();
            x.Strategies.Add<RemoveNavigatorAnimationStrategy>();
            x.Strategies.Add<InsertNavigatorAnimationStrategy>();
        }
    );
    serviceCollection.AddUGUIAsMVP(x =>
    {
        x.UGUIOption.PrefabViewManager = option.PrefabViewManager;
        x.PresenterLoaderFactoryType = option.PresenterLoaderFactoryType;
    });
    serviceCollection.AddScreenLifecycleEvent();
}).Configure(app =>
{
    // Middlewareを実行順序を定義します。
    app.UseScreenNavigatorEvent();
    app.UseInputLocker();
    app.UseScreenUI();
    app.UseNavigatorAnimation();
    app.UseUGUI();
    app.UseScreenLifecycleEvent();
}).Build();
```

## Navigation Animation
StackNavigatorは、画面遷移時にアニメーションを再生することができます。
アニメーションには以下の4つの概念があります。 
#### Open
新しいScreenが表示される時に再生されるアニメーションです。
#### Close
現在のScreenが破棄される時に再生されるアニメーションです。
#### Show
Pop等の遷移処理で、非アクティブになっていたScreenに戻る時に再生されるアニメーションです。
#### Hide
Push等の遷移処理で、現在のScreenが非アクティブになる時に再生されるアニメーションです。


### AnimationClipを使用したアニメーション
PresenterのPrefabのルートにAnimatorと専用のコンポーネントをアタッチすることで実現できます。
![MVP](Docs/Assets/NavigatorAnimationByAnimationClip.png)  

StackNavigatorは、画面遷移時にアニメーションを実行することができます。  
アニメーションは、AnimationClipを使用して設定し、 Presenter用のPrefabにAnimatorと専用のコンポーネントをアタッチすることで設定できます。
1. NavigatorAnimationPlayerをPrefabのRootノードにアタッチする
2. NavigatorTweenByAnimationClipをアタッチする
3. NavigatorTweenByAnimationClipのAnimationClipとNavigationTypeを設定する

> **ヒント**  
> アニメーションウィンドウを開くと、AnimationClipの調整が可能です(NavigatorAnimationPlayerノードを選択している必要があります)。


## Screen Lifecycle
#TODO
