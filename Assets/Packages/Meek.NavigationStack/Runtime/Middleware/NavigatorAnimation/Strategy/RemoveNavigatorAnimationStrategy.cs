using System.Collections;
using System.Linq;
using UnityEngine;
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
            return PlayRemoveAnimationRoutine(context);
        }

        private IEnumerator PlayRemoveAnimationRoutine(NavigationContext context)
        {
            var removeScreen = context.GetFeatureValue<StackScreen>(StackNavigationContextFeatureDefine.RemoveScreen);
            var beforeScreen = context.GetFeatureNullableValue<StackScreen>(StackNavigationContextFeatureDefine.RemoveBeforeScreen);
            var afterScreen = context.GetFeatureNullableValue<StackScreen>(StackNavigationContextFeatureDefine.RemoveAfterScreen);

            if (afterScreen != null && beforeScreen == null)
            {
                if (removeScreen.ScreenUIType == ScreenUIType.FullScreen)
                {
                    _screenContainer.SetVisibleBetweenTargetScreenToBeforeFullScreen(afterScreen, true);
                }
            }

            yield break;
        }
    }
}