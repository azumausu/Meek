using System.Collections;
using UnityEngine.Scripting;

namespace Meek.NavigationStack
{
    public class InsertNavigatorAnimationStrategy : INavigatorAnimationStrategy
    {
        private readonly IScreenContainer _screenContainer;

        [Preserve]
        public InsertNavigatorAnimationStrategy(IScreenContainer screenContainer)
        {
            _screenContainer = screenContainer;
        }

        bool INavigatorAnimationStrategy.IsValid(StackNavigationContext context)
        {
            return context.NavigatingSourceType == StackNavigationSourceType.Insert;
        }

        IEnumerator INavigatorAnimationStrategy.PlayAnimationRoutine(StackNavigationContext context)
        {
            // Animationの再生
            return PlayInsertAnimationRoutine(context);
        }

        private IEnumerator PlayInsertAnimationRoutine(NavigationContext context)
        {
            var insertionScreen = context.GetFeatureValue<StackScreen>(StackNavigationContextFeatureDefine.InsertionScreen);
            var insertionScreenType = insertionScreen.GetType();
            var beforeScreen = _screenContainer.GetScreenBefore(insertionScreenType) as StackScreen;
            var afterScreen = _screenContainer.GetScreenAfter(insertionScreenType) as StackScreen;
            var beforeScreenType = beforeScreen?.GetType();
            var afterScreenType = afterScreen?.GetType();

            if (afterScreen != null)
                yield return afterScreen.UI.HideRoutine(afterScreenType, insertionScreenType, true);
            yield return insertionScreen.UI.HideRoutine(insertionScreenType, beforeScreenType,  true);

            if (beforeScreen is { ScreenUIType: ScreenUIType.FullScreen }) insertionScreen.UI.SetVisible(false);
            else
            {
                insertionScreen.UI.SetVisible(true);
                if (insertionScreen.ScreenUIType != ScreenUIType.FullScreen)
                    _screenContainer.SetVisibleBetweenTargetScreenToBeforeFullScreen(insertionScreenType, true);
            }
            // AfterScreenはPeekにある場合は既に表示済みで何もしなくて良いし、Peekでない場合はAfterScreenのさらに上に載っている
            // ScreenによってPauseがかかっているので、こちらのパターンでも何もする必要がない
        }
    }
}