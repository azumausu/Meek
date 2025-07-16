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
            return PlayInsertAnimationRoutine(context);
        }

        private IEnumerator PlayInsertAnimationRoutine(NavigationContext context)
        {
            var insertionScreen = context.GetFeatureValue<StackScreen>(StackNavigationContextFeatureDefine.InsertionScreen);
            var beforeScreen = _screenContainer.GetScreenBefore(insertionScreen) as StackScreen;

            if (beforeScreen is { ScreenUIType: ScreenUIType.FullScreen }) insertionScreen.UI.SetVisible(false);
            else
            {
                insertionScreen.UI.SetVisible(true);
                if (insertionScreen.ScreenUIType != ScreenUIType.FullScreen)
                {
                    _screenContainer.SetVisibleBetweenTargetScreenToBeforeFullScreen(insertionScreen, true);
                }
            }

            yield break;
        }
    }
}