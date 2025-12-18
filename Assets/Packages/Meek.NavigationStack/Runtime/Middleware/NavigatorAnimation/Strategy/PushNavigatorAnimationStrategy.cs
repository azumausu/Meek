using System.Collections;
using UnityEngine.Pool;
using UnityEngine.Scripting;

namespace Meek.NavigationStack
{
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

        protected virtual IEnumerator PushTransition(StackNavigationContext context)
        {
            var toScreen = context.ToScreen as StackScreen;
            var fromScreen = context.FromScreen as StackScreen;
            var skipAnimation = context.SkipAnimation;
            var isCrossFade = context.IsCrossFade;

            // Noneの場合はイベントだけ発行して終了
            if (toScreen!.ScreenUIType == ScreenUIType.None) yield break;

            if (isCrossFade)
            {
                // CrossFadeの確認
                using var disposable = ListPool<IEnumerator>.Get(out var coroutines);

                // 次ScreenのVisibleをONにしておく
                toScreen!.UI.SetVisible(true);
                if (fromScreen != null)
                {
                    coroutines.Add(fromScreen.UI.HideRoutine(context, skipAnimation));
                }

                coroutines.Add(toScreen.UI.OpenRoutine(context, skipAnimation));
                yield return _coroutineRunner.StartParallelCoroutine(coroutines);
            }
            else
            {
                if (fromScreen != null)
                {
                    yield return fromScreen.UI.HideRoutine(context, skipAnimation);
                }

                toScreen!.UI.SetVisible(true);
                yield return toScreen.UI.OpenRoutine(context, skipAnimation);
            }

            // FullScreenUIが乗った時のみ、1つ下の全画面Viewが見つかるまで全て非表示にする。
            if (fromScreen != null && toScreen.ScreenUIType == ScreenUIType.FullScreen)
            {
                _screenContainer.SetVisibleBetweenTargetScreenToBeforeFullScreen(fromScreen, false);
            }
        }
    }
}