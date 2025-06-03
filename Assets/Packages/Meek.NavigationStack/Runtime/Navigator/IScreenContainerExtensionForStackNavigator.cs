using System;
using System.Linq;

namespace Meek.NavigationStack
{
    public static class IScreenContainerExtensionForStackNavigator
    {
        public static bool ShouldVisibleByUser(this IScreenContainer self, StackScreen target)
        {
            foreach (var screen in self.Screens.OfType<StackScreen>())
            {
                if (screen == target) return true;
                if (screen.ScreenUIType == ScreenUIType.FullScreen) return false;
            }

            return false;
        }

        public static void SetVisibleBetweenTargetScreenToBeforeFullScreen(this IScreenContainer self, IScreen targetScreen, bool visible)
        {
            var targetScreenIndex = self.Screens
                .Select((screen, index) => new { Screen = screen, Index = index, })
                .FirstOrDefault(x => x.Screen == targetScreen)?.Index ?? 0;
            foreach (var screen in self.Screens.OfType<StackScreen>().Skip(targetScreenIndex))
            {
                screen.UI.SetVisible(visible);
                if (screen.ScreenUIType == ScreenUIType.FullScreen) break;
            }
        }
    }
}