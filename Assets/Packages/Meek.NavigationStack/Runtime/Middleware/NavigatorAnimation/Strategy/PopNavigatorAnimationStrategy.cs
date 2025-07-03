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


        private IEnumerator PlayPopAnimationRoutine(StackNavigationContext context)
        {
            // toScreenは存在しない可能性がある。
            var fromScreen = (StackScreen)context.FromScreen;
            var toScreen = context.ToScreen as StackScreen;
            var fromScreenClassType = fromScreen.GetType();
            var toScreenClassType = toScreen?.GetType();
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
}