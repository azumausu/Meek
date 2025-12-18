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
            return PlayRemoveAnimationRoutine(context);
        }

        protected virtual IEnumerator PlayRemoveAnimationRoutine(StackNavigationContext context)
        {
            var removeScreen = context.GetRemoveScreen();
            var beforeScreen = context.GetRemoveBeforeScreen();
            var afterScreen = context.GetRemoveAfterScreen();

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