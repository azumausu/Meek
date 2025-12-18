using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Meek.NavigationStack
{
    public static class INavigatorTweenExtension
    {
        public static IEnumerable<INavigatorAnimation> MatchNavigatorAnimationType(
            this List<INavigatorAnimation> self,
            NavigatorAnimationType transitionType
        )
        {
            return self.Where(x => x.NavigatorAnimationType == transitionType);
        }

        public static IEnumerable<INavigatorAnimation> MatchFromScreenClassType(
            this List<INavigatorAnimation> self,
            Type fromScreenClassType
        )
        {
            return self.Where(x => !string.IsNullOrEmpty(x.FromScreenName) &&
                                   x.FromScreenName == fromScreenClassType.Name
            );
        }

        public static IEnumerable<INavigatorAnimation> MatchToScreenClassType(
            this List<INavigatorAnimation> self,
            Type toScreenClassType
        )
        {
            return self.Where(x => !string.IsNullOrEmpty(x.ToScreenName) &&
                                   x.ToScreenName == toScreenClassType.Name
            );
        }

        public static bool IsMatchNavigatorAnimationType(this INavigatorAnimation self, NavigatorAnimationType transitionType)
        {
            return self.NavigatorAnimationType == transitionType;
        }

        public static bool IsMatchFromScreenName(this INavigatorAnimation self, [CanBeNull] string fromScreenName)
        {
            return !string.IsNullOrEmpty(self.FromScreenName) && self.FromScreenName == fromScreenName;
        }

        public static bool IsMatchToScreenName(this INavigatorAnimation self, [CanBeNull] string toScreenName)
        {
            return !string.IsNullOrEmpty(self.ToScreenName) && self.ToScreenName == toScreenName;
        }
    }
}