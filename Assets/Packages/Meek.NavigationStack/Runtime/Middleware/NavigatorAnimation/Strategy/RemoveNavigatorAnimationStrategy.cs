using System.Collections;
using UnityEngine.Scripting;

namespace Meek.NavigationStack
{
    public class RemoveNavigatorAnimationStrategy : INavigatorAnimationStrategy
    {
        private readonly IScreenContainer _screenContainer;

        [Preserve]
        public RemoveNavigatorAnimationStrategy(IScreenContainer screenContainer)
        {
            _screenContainer = screenContainer;
        }

        bool INavigatorAnimationStrategy.IsValid(StackNavigationContext context)
        {
            return context.NavigatingSourceType == StackNavigationSourceType.Remove;
        }

        IEnumerator INavigatorAnimationStrategy.PlayAnimationRoutine(StackNavigationContext context)
        {
            // Animationの再生
            return PlayRemoveAnimationRoutine(context);
        }

        private IEnumerator PlayRemoveAnimationRoutine(NavigationContext context)
        {
            var removeScreen = context.GetFeatureValue<StackScreen>(StackNavigationContextFeatureDefine.RemoveScreen);
            var beforeScreen = context.GetFeatureNullableValue<StackScreen>(StackNavigationContextFeatureDefine.RemoveBeforeScreen);
            var afterScreen = context.GetFeatureNullableValue<StackScreen>(StackNavigationContextFeatureDefine.RemoveAfterScreen);
            var beforeScreenType = beforeScreen?.GetType();
            var afterScreenType = afterScreen?.GetType();

            if (afterScreenType != null)
            {
                yield return afterScreen.UI.HideRoutine(afterScreenType, beforeScreenType, true);
                if (removeScreen.ScreenUIType == ScreenUIType.FullScreen)
                {
                    _screenContainer.SetVisibleBetweenTargetScreenToBeforeFullScreen(afterScreen, true);
                }
            }
        }
    }
}