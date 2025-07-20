# Meek Framework Wiki

**Meek**は、スタックベースのナビゲーションを実装したMVPアーキテクチャによるDIベースのUnity UIフレームワークです。

## 📋 目次

### 🏗️ [フレームワーク概要](Framework-Overview)
- [パッケージ構成](Framework-Overview#パッケージ構成)
- [主要特徴](Framework-Overview#主要特徴)
- [アーキテクチャ概要](Framework-Overview#アーキテクチャ概要)

### 🧭 [ナビゲーションシステム](Navigation-System)
- [StackNavigationService](Navigation-System#stacknavigationservice)
- [スタック操作](Navigation-System#スタック操作)
- [パラメーター渡し](Navigation-System#パラメーター渡し)
- [Dispatch機能](Navigation-System#dispatch機能)

### 🏛️ [MVPアーキテクチャ](MVP-Architecture)
- [MVPScreen](MVP-Architecture#mvpscreen)
- [Presenter](MVP-Architecture#presenter)
- [Model管理](MVP-Architecture#model管理)
- [ライフサイクル](MVP-Architecture#ライフサイクル)

### ⚙️ [ミドルウェアシステム](Middleware-System)
- [パイプライン構造](Middleware-System#パイプライン構造)
- [標準ミドルウェア](Middleware-System#標準ミドルウェア)
- [カスタムミドルウェア作成](Middleware-System#カスタムミドルウェア作成)
- [アニメーション戦略](Middleware-System#アニメーション戦略)

### 🔧 [Unity統合](Unity-Integration)
- [UGUI統合](Unity-Integration#ugui統合)
- [VContainer統合](Unity-Integration#vcontainer統合)
- [リソース管理](Unity-Integration#リソース管理)

### 📚 [APIリファレンス](API-Reference)
- [コアインターフェース](API-Reference#コアインターフェース)
- [ライフサイクルイベント](API-Reference#ライフサイクルイベント)
- [設計原則](API-Reference#設計原則)

## 🚀 クイックスタート

### 1. アプリケーション初期化

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

### 2. Screen作成

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
            presenter.OnClickButton.Subscribe(_ => 
                PushNavigation.PushAsync<NextScreen>());
        });
    }
}
```

### 3. Presenter作成

```csharp
public class SplashPresenter : Presenter<SplashModel>
{
    [SerializeField] private Button _button;

    public IObservable<Unit> OnClickButton => _button.OnClickAsObservable();

    protected override IEnumerable<IDisposable> Bind(SplashModel model)
    {
        // モデルとビューのバインディング
        yield break;
    }
}
```

## 📖 詳細な使用方法

各セクションの詳細については、サイドバーのリンクから該当ページをご確認ください。

## 🔗 関連リンク

- [GitHub Repository](https://github.com/your-repo/meek)
- [Unity Asset Store](#)
- [Developer Documentation](#)

---

*最終更新: 2025年7月*