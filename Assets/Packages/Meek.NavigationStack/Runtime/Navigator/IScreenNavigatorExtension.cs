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
            return self.Screens.FirstOrDefault(x => x.GetType() == targetScreenType)
                   ?? throw new InvalidOperationException($"{targetScreenType.Name} does not exist.");
        }

        [CanBeNull]
        public static IScreen GetPeekScreen(this IScreenContainer self)
        {
            return self.Screens.FirstOrDefault();
        }
    }
}