using System;
using System.Linq;
using JetBrains.Annotations;

namespace Meek.NavigationStack
{
    public static class IScreenNavigatorExtension
    {
        [CanBeNull]
        public static IScreen GetScreenBefore(this IScreenContainer self, IScreen beforeScreen)
        {
            IScreen targetScreen = null;
            foreach (var screen in self.Screens)
            {
                if (beforeScreen == screen) return targetScreen;
                targetScreen = screen;
            }

            return null;
        }

        [CanBeNull]
        public static IScreen GetScreenAfter(this IScreenContainer self, IScreen afterScreen)
        {
            var exist = false;
            foreach (var screen in self.Screens)
            {
                if (exist) return screen;
                exist = afterScreen == screen;
            }

            return null;
        }

        [CanBeNull]
        public static IScreen GetScreen<TScreen>(this IScreenContainer self)
        {
            var target = typeof(TScreen);
            return self.GetScreen(target);
        }

        [CanBeNull]
        public static IScreen GetScreen(this IScreenContainer self, Type targetScreenType)
        {
            foreach (var screen in self.Screens)
            {
                if (screen.GetType() == targetScreenType) return screen;
            }

            throw new Exception($"Can't find screen type of {targetScreenType.Name}");
        }

        [CanBeNull]
        public static IScreen GetPeekScreen(this IScreenContainer self)
        {
            if (self.Screens.Count == 0) return null;
            return self.Screens.First();
        }
    }
}