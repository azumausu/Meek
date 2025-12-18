using System.Collections;
using UnityEngine.Pool;
using UnityEngine.Scripting;

namespace Meek.NavigationStack
{
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

        /// <summary>
        /// trueの場合このAnimationを実行します
        /// </summary>
        bool INavigatorAnimationStrategy.IsValid(StackNavigationContext context)
        {
            return context.NavigatingSourceType == StackNavigationSourceType.Pop;
        }

        /// <summary>
        /// Stackの変更後の処理
        /// </summary>
        IEnumerator INavigatorAnimationStrategy.PlayAnimationRoutine(StackNavigationContext context)
        {
            // Animationの再生
            return PlayPopAnimationRoutine(context);
        }


        protected virtual IEnumerator PlayPopAnimationRoutine(StackNavigationContext context)
        {
            // toScreenは存在しない可能性がある。
            var fromScreen = context.FromScreen as StackScreen ?? throw new System.InvalidOperationException();
            var toScreen = context.ToScreen as StackScreen;
            var skipAnimation = context.SkipAnimation;
            var isCrossFade = context.IsCrossFade;

            // 破棄されるScreenが全てNoneの場合は何もせずに終了。
            // 破棄Screenは必ず1個以上存在するのでAllの判定で十分
            if (fromScreen.ScreenUIType == ScreenUIType.None) yield break;

            // 遷移後にユーザーに見えるUIのVisibleをOnにする（遷移後のScreenから最初のFullScreenUIまで）
            if (toScreen != null)
            {
                toScreen.UI.SetVisible(true);
                _screenContainer.SetVisibleBetweenTargetScreenToBeforeFullScreen(toScreen, true);
            }

            if (isCrossFade)
            {
                using var disposable = ListPool<IEnumerator>.Get(out var coroutines);

                coroutines.Add(fromScreen.UI.CloseRoutine(context, skipAnimation));
                if (toScreen != null)
                {
                    coroutines.Add(toScreen.UI.ShowRoutine(context, skipAnimation));
                }

                yield return _coroutineRunner.StartParallelCoroutine(coroutines);
            }
            else
            {
                yield return _coroutineRunner.StartCoroutine(fromScreen.UI.CloseRoutine(context, skipAnimation));
                if (toScreen != null)
                {
                    yield return toScreen.UI.ShowRoutine(context, skipAnimation);
                }
            }
        }
    }
}