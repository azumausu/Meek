using System.Collections;
using UnityEngine.Pool;
using UnityEngine.Scripting;

namespace Meek.NavigationStack
{
    public class PushNavigatorAnimationStrategy : INavigatorAnimationStrategy
    {
        private readonly IScreenContainer _screenContainer;
        private readonly CoroutineRunner _coroutineRunner;
        
        [Preserve]
        public PushNavigatorAnimationStrategy(IScreenContainer screenContainer, CoroutineRunner coroutineRunner)
        {
            _screenContainer = screenContainer;
            _coroutineRunner = coroutineRunner;
        }
        
        /// <summary>
        /// trueの場合このAnimationを実行します
        /// </summary>
        bool INavigatorAnimationStrategy.IsValid(StackNavigationContext context)
        {
            return context.NavigatingSourceType == StackNavigationSourceType.Push;
        }

        IEnumerator INavigatorAnimationStrategy.PlayAnimationRoutine(StackNavigationContext context)
        {
            // Animationの再生
            return PushTransition(context); 
        }
        
        private IEnumerator PushTransition(StackNavigationContext context)
        {
            var toScreenType = context.ToScreen.GetType();
            var fromScreenType = context.FromScreen?.GetType();
            var toScreen = context.ToScreen as StackScreen;
            var fromScreen = context.FromScreen as StackScreen;
            var skipAnimation = context.SkipAnimation;
            var isCrossFade = context.IsCrossFade;
            
            // TODO: 失敗のハンドリングは後ほど検討
            // // 次Screenの初期化に失敗した場合は遷移時のUIの表示は何も変更しない。
            // if (toScreen.IsInitializationFailed) yield break;
            
            // Noneの場合はイベントだけ発行して終了
            if (toScreen!.ScreenUIType == ScreenUIType.None) yield break;

            if (isCrossFade)
            {
                // CrossFadeの確認
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
                if (fromScreen != null)
                    yield return fromScreen.UI.HideRoutine(fromScreenType, toScreenType, skipAnimation);
                toScreen!.UI.SetVisible(true);
                yield return toScreen.UI.OpenRoutine(fromScreenType, toScreenType, skipAnimation);
            }

            // FullScreenUIが乗った時のみ、1つ下の全画面Viewが見つかるまで全て非表示にする。
            if (fromScreen != null && toScreen.ScreenUIType == ScreenUIType.FullScreen)
                _screenContainer.SetVisibleBetweenTargetScreenToBeforeFullScreen(fromScreenType, false);
        }

    }
}