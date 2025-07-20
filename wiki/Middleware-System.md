# ミドルウェアシステム

Meekのミドルウェアシステムは、ナビゲーション処理をパイプライン化し、拡張可能で柔軟なアーキテクチャを提供します。

## パイプライン構造

### NavigationDelegate

ミドルウェアチェーンの基本となる関数型デリゲート。

**場所**: `Meek/Runtime/Navigator/NavigationDelegate.cs`

```csharp
public delegate ValueTask NavigationDelegate(NavigationContext context);
```

### IMiddleware インターフェース

**場所**: `Meek/Runtime/IMiddleware.cs`

```csharp
public interface IMiddleware
{
    ValueTask InvokeAsync(NavigationContext context, NavigationDelegate next);
}
```

### UseMiddleware拡張メソッド

**場所**: `Meek/Runtime/Navigator/INavigatorBuilderExtension.cs:7`

```csharp
public static INavigatorBuilder UseMiddleware<T>(this INavigatorBuilder self) where T : IMiddleware
{
    return self.Use(next =>
    {
        return async context =>
        {
            var middleware = self.ServiceProvider.GetService<T>();
            await middleware.InvokeAsync(context, next);  // 入れ子構造を形成
        };
    });
}
```

### NavigatorBuilder による構築

**場所**: `Meek/Runtime/Navigator/NavigatorBuilder.cs:33`

```csharp
public INavigator Build()
{
    // 基盤の一番最初に呼び出すMiddleware（最終的な処理）
    var stackScreenManager = ServiceProvider.GetService<IScreenContainer>();
    NavigationDelegate app = stackScreenManager.NavigateAsync;
    
    // 逆順でミドルウェアをラップ（入れ子構造を構築）
    for (int i = _components.Count - 1; i >= 0; i--)
    {
        app = _components[i].Invoke(app);
    }
    
    return new Navigator(ServiceProvider, app);
}
```

## 標準ミドルウェア

### 登録順序と実行フロー

**登録順序** (`MVPApplication.cs:52`):
```csharp
app.UseDebug();                    // 1. デバッグ情報収集（最外層）
app.UseScreenNavigatorEvent();     // 2. ナビゲーションイベント制御
app.UseInputLocker();              // 3. 入力ロック
app.UseScreenUI();                 // 4. UI生成・破棄
app.UseNavigatorAnimation();       // 5. アニメーション制御
app.UseUGUI();                     // 6. Unity uGUI統合
app.UseScreenLifecycleEvent();     // 7. ライフサイクルイベント（最内層）
```

### ミドルウェアの入れ子実行フロー

ミドルウェアは**入れ子構造**で実行されます。各ミドルウェアは`await next(context)`で次のミドルウェアを呼び出し、その前後で処理を行います。

**実行フロー例**（Push操作時）:
```
1. Debug → 前処理開始 → next()へ
  2. ScreenNavigatorEvent → セマフォ取得 → ScreenWillNavigate → next()へ
    3. InputLocker → 入力ロック取得 → next()へ
      4. ScreenUI → ToScreen.Initialize() → next()へ
        5. NavigatorAnimation → next()へ
          6. UGUI → next()へ
            7. ScreenLifecycleEvent → StartingImpl実行 → next()へ
              8. StackScreenContainer → スタックにPush実行
            7. ← ScreenDidStart等のイベント実行
          6. ← UGUI後処理実行
        5. ← ViewWillOpen → アニメーション実行 → ViewDidOpen
      4. ← ScreenUI後処理（Pop/Remove時のみDispose処理）
    3. ← using自動解放で入力ロック解除
  2. ← ScreenDidNavigate → セマフォ解放
1. ← Debug後処理
```

**重要なポイント**:
- **await next(context)**より前 = 前処理
- **await next(context)**より後 = 後処理
- 各ミドルウェアは前処理→次のミドルウェア→後処理の順で実行
- 最も外側（最初に登録）のミドルウェアが最初と最後に実行される

### ScreenNavigatorEventMiddleware

スレッドセーフなナビゲーション制御を提供。

**場所**: `Meek.NavigationStack/Runtime/Middleware/ScreeenNavigatorEvent/ScreenNavigatorEventMiddleware.cs:9`

```csharp
public class ScreenNavigatorEventMiddleware : IMiddleware
{
    private readonly SemaphoreSlim _navigationLock = new SemaphoreSlim(1, 1);
    
    public async ValueTask InvokeAsync(NavigationContext context, NavigationDelegate next)
    {
        await _navigationLock.WaitAsync();
        try
        {
            // 遷移処理開始
            if (context.FromScreen is IScreenNavigatorEventHandler fromHandler)
                fromHandler.ScreenWillNavigate(context);

            await next(context);

            // 遷移処理完了
            if (context.ToScreen is IScreenNavigatorEventHandler toHandler)
                toHandler.ScreenDidNavigate(context);
        }
        finally { _navigationLock.Release(); }
    }
}
```

**役割**:
- `SemaphoreSlim`による並行ナビゲーション制御
- 遷移開始・完了の通知
- デッドロック防止のためのfinally確実解放

### InputLockerMiddleware

遷移中の入力をブロックする。

```csharp
public class InputLockerMiddleware : IMiddleware
{
    private readonly IInputLocker _inputLocker;

    public async ValueTask InvokeAsync(NavigationContext context, NavigationDelegate next)
    {
        using var locker = _inputLocker.LockInput();  // usingによる自動解放
        await next(context);
    }
}
```

**特徴**:
- `IDisposable`パターンによる確実な解放
- 遷移中のユーザー操作をブロック
- 例外発生時も確実に入力ロック解除

### ScreenUIMiddleware

UI生成・破棄とリソース管理。

```csharp
public class ScreenUIMiddleware : IMiddleware
{
    private readonly IScreenContainer _screenContainer;

    public async ValueTask InvokeAsync(NavigationContext context, NavigationDelegate next)
    {
        var stackContext = context.ToStackNavigationContext();
        
        // Push時の初期化
        if (stackContext.NavigatingSourceType == StackNavigationSourceType.Push)
        {
            stackContext.ToScreen.Initialize(context);
        }

        // Insert時の初期化
        if (stackContext.NavigatingSourceType == StackNavigationSourceType.Insert)
        {
            var insertionScreen = stackContext.GetFeatureValue<IScreen>(StackNavigationContextFeatureDefine.InsertionScreen);
            insertionScreen.Initialize(context);
        }
        
        await next(context);
        
        // Pop時の破棄処理
        if (stackContext.NavigatingSourceType == StackNavigationSourceType.Pop)
        {
            // IAsyncDisposable → IDisposable の順で破棄
            if (context.FromScreen is IAsyncDisposable asyncDisposable)
                await asyncDisposable.DisposeAsync();
            if (context.FromScreen is IDisposable disposable)
                disposable.Dispose();
        }

        // Remove時の破棄処理
        if (stackContext.NavigatingSourceType == StackNavigationSourceType.Remove)
        {
            var removeScreen = context.GetFeatureValue<IScreen>(StackNavigationContextFeatureDefine.RemoveScreen);

            if (removeScreen is IAsyncDisposable asyncDisposable)
                await asyncDisposable.DisposeAsync();
            if (removeScreen is IDisposable disposable)
                disposable.Dispose();
        }
    }
}
```

**責務**:
- Push時のToScreen初期化
- Insert時のInsertionScreen初期化  
- Pop時のFromScreen破棄
- Remove時のRemoveScreen破棄
- 非同期破棄とスタンダード破棄の両対応

### NavigatorAnimationMiddleware

遷移アニメーションとViewイベント管理。

**場所**: `Meek.NavigationStack/Runtime/Middleware/NavigatorAnimation/Middleware/NavigatorAnimationMiddleware.cs:9`

```csharp
public class NavigatorAnimationMiddleware : IMiddleware
{
    private readonly IScreenContainer _screenContainer;
    private readonly ICoroutineRunner _coroutineRunner;
    private readonly List<INavigatorAnimationStrategy> _transitionAnimationModules = new();

    [Preserve]
    public NavigatorAnimationMiddleware(
        IScreenContainer screenContainer,
        ICoroutineRunner coroutineRunner,
        PushNavigatorAnimationStrategy pushNavigatorAnimationStrategy,
        PopNavigatorAnimationStrategy popNavigatorAnimationStrategy,
        RemoveNavigatorAnimationStrategy removeNavigatorAnimationStrategy,
        InsertNavigatorAnimationStrategy insertNavigatorAnimationStrategy
    )
    {
        _screenContainer = screenContainer;
        _coroutineRunner = coroutineRunner;

        _transitionAnimationModules.Add(pushNavigatorAnimationStrategy);
        _transitionAnimationModules.Add(popNavigatorAnimationStrategy);
        _transitionAnimationModules.Add(removeNavigatorAnimationStrategy);
        _transitionAnimationModules.Add(insertNavigatorAnimationStrategy);
    }
    
    public async ValueTask InvokeAsync(NavigationContext context, NavigationDelegate next)
    {
        var stackContext = context.ToStackNavigationContext();

        await next(context); // 先にスタック操作を実行

        // ViewWillClose イベント発火 (Pop/Remove時)
        if (stackContext.NavigatingSourceType == StackNavigationSourceType.Pop)
        {
            if (context.FromScreen is not StackScreen fromUIScreen) throw new InvalidOperationException();
            fromUIScreen.ScreenEventInvoker.Invoke(NavigatorAnimationScreenEvent.ViewWillClose);
            await fromUIScreen.ScreenEventInvoker.InvokeAsync(NavigatorAnimationScreenEvent.ViewWillClose);
        }

        if (stackContext.NavigatingSourceType == StackNavigationSourceType.Remove)
        {
            var removeScreen = context.GetFeatureValue<IScreen>(StackNavigationContextFeatureDefine.RemoveScreen);
            if (removeScreen is not StackScreen removeUIScreen) throw new InvalidOperationException();
            removeUIScreen.ScreenEventInvoker.Invoke(NavigatorAnimationScreenEvent.ViewWillClose);
            await removeUIScreen.ScreenEventInvoker.InvokeAsync(NavigatorAnimationScreenEvent.ViewWillClose);
        }

        // ViewWillOpen イベント発火 (Push/Insert時)
        if (stackContext.NavigatingSourceType == StackNavigationSourceType.Push)
        {
            if (context.ToScreen is not StackScreen toUIScreen) throw new InvalidOperationException();
            toUIScreen.ScreenEventInvoker.Invoke(NavigatorAnimationScreenEvent.ViewWillOpen);
            await toUIScreen.ScreenEventInvoker.InvokeAsync(NavigatorAnimationScreenEvent.ViewWillOpen);
        }

        if (stackContext.NavigatingSourceType == StackNavigationSourceType.Insert)
        {
            if (context.ToScreen is not StackScreen toUIScreen) throw new InvalidOperationException();
            toUIScreen.ScreenEventInvoker.Invoke(NavigatorAnimationScreenEvent.ViewWillOpen);
            await toUIScreen.ScreenEventInvoker.InvokeAsync(NavigatorAnimationScreenEvent.ViewWillOpen);
        }
        
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
        if (stackContext.NavigatingSourceType == StackNavigationSourceType.Push)
        {
            if (context.ToScreen is not StackScreen toUIScreen) throw new InvalidOperationException();
            toUIScreen.ScreenEventInvoker.Invoke(NavigatorAnimationScreenEvent.ViewDidOpen);
            await toUIScreen.ScreenEventInvoker.InvokeAsync(NavigatorAnimationScreenEvent.ViewDidOpen);
        }

        if (stackContext.NavigatingSourceType == StackNavigationSourceType.Insert)
        {
            if (context.ToScreen is not StackScreen toUIScreen) throw new InvalidOperationException();
            toUIScreen.ScreenEventInvoker.Invoke(NavigatorAnimationScreenEvent.ViewDidOpen);
            await toUIScreen.ScreenEventInvoker.InvokeAsync(NavigatorAnimationScreenEvent.ViewDidOpen);
        }

        if (stackContext.NavigatingSourceType == StackNavigationSourceType.Pop)
        {
            if (context.FromScreen is not StackScreen fromUIScreen) throw new InvalidOperationException();
            fromUIScreen.ScreenEventInvoker.Invoke(NavigatorAnimationScreenEvent.ViewDidClose);
            await fromUIScreen.ScreenEventInvoker.InvokeAsync(NavigatorAnimationScreenEvent.ViewDidClose);
        }

        if (stackContext.NavigatingSourceType == StackNavigationSourceType.Remove)
        {
            var removeScreen = context.GetFeatureValue<IScreen>(StackNavigationContextFeatureDefine.RemoveScreen);
            if (removeScreen is not StackScreen removeUIScreen) throw new InvalidOperationException();
            removeUIScreen.ScreenEventInvoker.Invoke(NavigatorAnimationScreenEvent.ViewDidClose);
            await removeUIScreen.ScreenEventInvoker.InvokeAsync(NavigatorAnimationScreenEvent.ViewDidClose);
        }
    }
}
```

**機能**:
- 4つのアニメーション戦略をDIで注入
- 操作タイプ別のViewWillOpen/ViewWillCloseイベント発火
- 同期・非同期両方のイベント発火
- Strategy パターンによる遷移アニメーション
- Unity Coroutine と async/await の統合

## アニメーション戦略

### INavigatorAnimationStrategy

```csharp
public interface INavigatorAnimationStrategy
{
    bool IsValid(StackNavigationContext context);           // 実行条件判定
    IEnumerator PlayAnimationRoutine(StackNavigationContext context);  // アニメーション実行
}
```

### Push戦略実装

```csharp
public class PushNavigatorAnimationStrategy : INavigatorAnimationStrategy
{
    private readonly IScreenContainer _screenContainer;
    private readonly ICoroutineRunner _coroutineRunner;

    [Preserve]
    public PushNavigatorAnimationStrategy(IScreenContainer screenContainer, ICoroutineRunner coroutineRunner)
    {
        _screenContainer = screenContainer;
        _coroutineRunner = coroutineRunner;
    }

    bool INavigatorAnimationStrategy.IsValid(StackNavigationContext context)
    {
        return context.NavigatingSourceType == StackNavigationSourceType.Push;
    }

    IEnumerator INavigatorAnimationStrategy.PlayAnimationRoutine(StackNavigationContext context)
    {
        var toScreenType = context.ToScreen.GetType();
        var fromScreenType = context.FromScreen?.GetType();
        var toScreen = context.ToScreen as StackScreen;
        var fromScreen = context.FromScreen as StackScreen;
        var skipAnimation = context.SkipAnimation;
        var isCrossFade = context.IsCrossFade;

        // Noneの場合はイベントだけ発行して終了
        if (toScreen!.ScreenUIType == ScreenUIType.None) yield break;

        if (isCrossFade)
        {
            // CrossFadeの場合：並列実行
            var coroutines = ListPool<IEnumerator>.Get();

            // 次ScreenのVisibleをONにしておく
            toScreen!.UI.SetVisible(true);
            coroutines.Add(fromScreen.UI.HideRoutine(fromScreenType, toScreenType, skipAnimation));
            coroutines.Add(toScreen.UI.OpenRoutine(fromScreenType, toScreenType, skipAnimation));
            yield return _coroutineRunner.StartParallelCoroutine(coroutines.ToArray());

            ListPool<IEnumerator>.Release(coroutines);
        }
        else
        {
            // 順次実行：Hide(From) → Open(To)
            if (fromScreen != null)
            {
                yield return fromScreen.UI.HideRoutine(fromScreenType, toScreenType, skipAnimation);
            }

            toScreen!.UI.SetVisible(true);
            yield return toScreen.UI.OpenRoutine(fromScreenType, toScreenType, skipAnimation);
        }

        // FullScreenUIが乗った時のみ、1つ下の全画面Viewが見つかるまで全て非表示にする。
        if (fromScreen != null && toScreen.ScreenUIType == ScreenUIType.FullScreen)
        {
            _screenContainer.SetVisibleBetweenTargetScreenToBeforeFullScreen(fromScreen, false);
        }
    }
}
```

### Pop戦略実装

```csharp
public class PopNavigatorAnimationStrategy : INavigatorAnimationStrategy
{
    private readonly IScreenContainer _screenContainer;
    private readonly ICoroutineRunner _coroutineRunner;

    [Preserve]
    public PopNavigatorAnimationStrategy(IScreenContainer screenContainer, ICoroutineRunner coroutineRunner)
    {
        _screenContainer = screenContainer;
        _coroutineRunner = coroutineRunner;
    }

    bool INavigatorAnimationStrategy.IsValid(StackNavigationContext context)
    {
        return context.NavigatingSourceType == StackNavigationSourceType.Pop;
    }

    IEnumerator INavigatorAnimationStrategy.PlayAnimationRoutine(StackNavigationContext context)
    {
        // toScreenは存在しない可能性がある。
        var fromScreen = (StackScreen)context.FromScreen;
        var toScreen = context.ToScreen as StackScreen;
        var fromScreenClassType = fromScreen.GetType();
        var toScreenClassType = toScreen?.GetType();
        var skipAnimation = context.SkipAnimation;
        var isCrossFade = context.IsCrossFade;

        // 破棄されるScreenが全てNoneの場合は何もせずに終了。
        if (fromScreen.ScreenUIType == ScreenUIType.None) yield break;

        // 遷移後にユーザーに見えるUIのVisibleをOnにする（遷移後のScreenから最初のFullScreenUIまで）
        if (toScreen != null)
        {
            toScreen.UI.SetVisible(true);
            _screenContainer.SetVisibleBetweenTargetScreenToBeforeFullScreen(toScreen, true);
        }

        if (isCrossFade)
        {
            // CrossFadeの場合：並列実行
            var coroutines = ListPool<IEnumerator>.Get();

            coroutines.Add(fromScreen.UI.CloseRoutine(fromScreenClassType, toScreenClassType, skipAnimation));
            if (toScreen != null)
            {
                coroutines.Add(toScreen.UI.ShowRoutine(fromScreenClassType, toScreenClassType, skipAnimation));
            }

            yield return _coroutineRunner.StartParallelCoroutine(coroutines);

            ListPool<IEnumerator>.Release(coroutines);
        }
        else
        {
            // 順次実行：Close(From) → Show(To)
            yield return _coroutineRunner.StartCoroutine(
                fromScreen.UI.CloseRoutine(fromScreenClassType, toScreenClassType, skipAnimation)
            );
            if (toScreen != null)
            {
                yield return toScreen.UI.ShowRoutine(fromScreenClassType, toScreenClassType, skipAnimation);
            }
        }
    }
}
```

### アニメーション種類

```csharp
public enum NavigatorAnimationType
{
    /// <summary>
    /// UI生成時
    /// </summary>
    Open,
    
    /// <summary>
    /// UI破棄時
    /// </summary>
    Close,
    
    /// <summary>
    /// Pause復帰時
    /// </summary>
    Show,
    
    /// <summary>
    /// Pause時
    /// </summary>
    Hide,
}
```

### ScreenUIType

```csharp
public enum ScreenUIType
{
    None,                   // UIなし（アニメーション実行されない）
    FullScreen,            // フルスクリーンUI（背後のUIを非表示にする）
    WindowOrTransparent,   // ウィンドウ表示または下のUIが透過されるUI
}
```

**実装の特徴**:
- `ScreenUIType.None` の場合、アニメーション処理をスキップ
- `ScreenUIType.FullScreen` の場合、背後の画面を自動的に非表示
- UI.OpenRoutine()などは実際には3つの引数を受け取る：
  - `fromScreenType`: 遷移元画面の型
  - `toScreenType`: 遷移先画面の型  
  - `skipAnimation`: アニメーションスキップフラグ
- ObjectPool (`ListPool<IEnumerator>`) による効率的なメモリ管理
- `SetVisible(true)` による適切な表示状態管理

## カスタムミドルウェア作成

### 基本的なミドルウェア

```csharp
public class LoggingMiddleware : IMiddleware
{
    private readonly ILogger _logger;

    public LoggingMiddleware(ILogger logger)
    {
        _logger = logger;
    }

    public async ValueTask InvokeAsync(NavigationContext context, NavigationDelegate next)
    {
        var stackContext = context.ToStackNavigationContext();
        
        // 前処理
        _logger.LogInfo($"Navigation starting: {stackContext.NavigatingSourceType}");
        _logger.LogInfo($"From: {context.FromScreen?.GetType().Name}");
        _logger.LogInfo($"To: {context.ToScreen?.GetType().Name}");

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            // 次のミドルウェア実行
            await next(context);
            
            // 後処理
            stopwatch.Stop();
            _logger.LogInfo($"Navigation completed in {stopwatch.ElapsedMilliseconds}ms");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Navigation failed: {ex.Message}");
            throw;
        }
    }
}
```

### 条件付きミドルウェア

```csharp
public class ConditionalMiddleware : IMiddleware
{
    public async ValueTask InvokeAsync(NavigationContext context, NavigationDelegate next)
    {
        var stackContext = context.ToStackNavigationContext();
        
        // 特定条件でのみ処理を実行
        if (stackContext.NavigatingSourceType == StackNavigationSourceType.Push)
        {
            // Push時のみの特別処理
            await HandlePushSpecificLogic(context);
        }
        
        await next(context);
        
        // 後処理も条件付きで
        if (stackContext.IsCrossFade)
        {
            await HandleCrossFadeCleanup(context);
        }
    }
}
```

### パフォーマンス監視ミドルウェア

```csharp
public class PerformanceMonitoringMiddleware : IMiddleware
{
    private readonly IPerformanceTracker _tracker;

    public async ValueTask InvokeAsync(NavigationContext context, NavigationDelegate next)
    {
        var stackContext = context.ToStackNavigationContext();
        var operationName = $"Navigation_{stackContext.NavigatingSourceType}";
        
        using var operation = _tracker.StartOperation(operationName);
        
        // メモリ使用量の記録
        var beforeMemory = GC.GetTotalMemory(false);
        
        await next(context);
        
        // パフォーマンスメトリクスの記録
        var afterMemory = GC.GetTotalMemory(false);
        var memoryDelta = afterMemory - beforeMemory;
        
        _tracker.RecordMetric("memory_delta", memoryDelta);
        _tracker.RecordMetric("screens_in_stack", 
            context.ToStackNavigationContext().ScreenContainer.Screens.Count);
    }
}
```

## 拡張メソッドの作成

### ミドルウェア登録の拡張

```csharp
public static class CustomMiddlewareExtensions
{
    public static IServiceCollection AddLoggingMiddleware(this IServiceCollection self)
    {
        self.AddSingleton<LoggingMiddleware>();
        return self;
    }
    
    public static INavigatorBuilder UseLoggingMiddleware(this INavigatorBuilder app)
    {
        return app.UseMiddleware<LoggingMiddleware>();
    }
    
    public static INavigatorBuilder UsePerformanceMonitoring(this INavigatorBuilder app)
    {
        return app.UseMiddleware<PerformanceMonitoringMiddleware>();
    }
}
```

### 使用例

```csharp
var navigator = new NavigatorBuilder(containerBuilder)
    .ConfigureServices(services => 
    {
        services.AddLoggingMiddleware();
        services.AddSingleton<IPerformanceTracker, PerformanceTracker>();
    })
    .Configure(app => 
    {
        app.UseDebug();
        app.UseLoggingMiddleware();           // カスタムミドルウェア
        app.UsePerformanceMonitoring();       // カスタムミドルウェア
        app.UseScreenNavigatorEvent();
        app.UseInputLocker();
        app.UseScreenUI();
        app.UseNavigatorAnimation();
        app.UseUGUI();
        app.UseScreenLifecycleEvent();
    })
    .Build();
```

## Unity Coroutine統合

### CoroutineRunner

Unity CoroutineとAsync/Awaitの橋渡し。

```csharp
public class CoroutineRunner : ICoroutineRunner
{
    public void StartCoroutineWithCallback(IEnumerator routine, Action onComplete)
    {
        var component = _coroutineRunnerComponent;
        component.StartCoroutine(ExecuteWithCallback(routine, onComplete));
    }

    private IEnumerator ExecuteWithCallback(IEnumerator routine, Action onComplete)
    {
        yield return routine;
        onComplete?.Invoke();
    }

    public IEnumerator StartParallelCoroutine(params IEnumerator[] routines)
    {
        if (routines == null || routines.Length == 0) yield break;

        var coroutines = routines.Where(r => r != null)
                                .Select(r => _coroutineRunnerComponent.StartCoroutine(r))
                                .ToArray();

        foreach (var coroutine in coroutines)
        {
            yield return coroutine;
        }
    }
}
```

### 非同期アニメーション実行

```csharp
// NavigatorAnimationMiddleware.cs:242
var strategy = _transitionAnimationModules.LastOrDefault(x => x.IsValid(stackContext));
if (strategy != null)
{
    var tcs = new TaskCompletionSource<bool>();
    _coroutineRunner.StartCoroutineWithCallback(strategy.PlayAnimationRoutine(stackContext),
        () => tcs.SetResult(true));
    await tcs.Task;
}
```

**重要なポイント**:
- `LastOrDefault()` により、後に登録された戦略が優先される
- `TaskCompletionSource<bool>` によりCoroutineの完了をawaitで待機
- 戦略が見つからない場合はアニメーションをスキップ

## ミドルウェア設計パターン

### 責任の分離

各ミドルウェアは単一の責任を持つ：

- **InputLocker**: 入力制御のみ
- **ScreenUI**: UI生成・破棄のみ
- **NavigatorAnimation**: アニメーション実行のみ

### 失敗時の処理

```csharp
public class RobustMiddleware : IMiddleware
{
    public async ValueTask InvokeAsync(NavigationContext context, NavigationDelegate next)
    {
        try
        {
            await ExecutePreProcessing(context);
            await next(context);
            await ExecutePostProcessing(context);
        }
        catch (Exception ex)
        {
            // ミドルウェア固有のエラーハンドリング
            await HandleError(context, ex);
            throw; // 上位に例外を再スロー
        }
        finally
        {
            // クリーンアップ処理（必ず実行）
            await Cleanup(context);
        }
    }
}
```

### リソース管理

```csharp
public class ResourceManagedMiddleware : IMiddleware
{
    public async ValueTask InvokeAsync(NavigationContext context, NavigationDelegate next)
    {
        var resource = AcquireResource();
        try
        {
            await next(context);
        }
        finally
        {
            resource?.Dispose(); // 確実にリソース解放
        }
    }
}
```

## デバッグとトラブルシューティング

### DebugMiddleware

開発時のナビゲーション情報収集。

```csharp
public class NavigationStackDebugMiddleware : IMiddleware
{
    public async ValueTask InvokeAsync(NavigationContext context, NavigationDelegate next)
    {
        var stackContext = context.ToStackNavigationContext();
        
        // 実行前の状態ログ
        LogNavigationState("Before", stackContext);
        
        await next(context);
        
        // 実行後の状態ログ
        LogNavigationState("After", stackContext);
    }

    private void LogNavigationState(string phase, StackNavigationContext context)
    {
        Debug.Log($"[{phase}] Operation: {context.NavigatingSourceType}");
        Debug.Log($"[{phase}] Stack Count: {context.ScreenContainer.Screens.Count}");
        Debug.Log($"[{phase}] Current Screens: {string.Join(", ", 
            context.ScreenContainer.Screens.Select(s => s.GetType().Name))}");
    }
}
```

### パフォーマンス分析

```csharp
public class PerformanceAnalysisMiddleware : IMiddleware
{
    public async ValueTask InvokeAsync(NavigationContext context, NavigationDelegate next)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var memoryBefore = GC.GetTotalMemory(false);

        await next(context);

        stopwatch.Stop();
        var memoryAfter = GC.GetTotalMemory(false);
        var memoryDelta = memoryAfter - memoryBefore;

        if (stopwatch.ElapsedMilliseconds > 100) // 100ms以上かかった場合
        {
            Debug.LogWarning($"Slow navigation detected: {stopwatch.ElapsedMilliseconds}ms");
        }

        if (memoryDelta > 1024 * 1024) // 1MB以上のメモリ増加
        {
            Debug.LogWarning($"High memory allocation: {memoryDelta / 1024 / 1024}MB");
        }
    }
}
```

## 次のステップ

- [Unity統合](Unity-Integration) - uGUIとVContainerの統合詳細
- [APIリファレンス](API-Reference) - 完全なAPIドキュメント

---

[[← MVP Architecture]](MVP-Architecture) | [[Unity Integration →]](Unity-Integration)