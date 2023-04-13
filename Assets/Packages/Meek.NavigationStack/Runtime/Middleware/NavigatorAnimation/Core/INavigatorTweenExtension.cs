using System;
using System.Collections.Generic;
using System.Linq;

namespace Meek.NavigationStack
{
    public static class INavigatorTweenExtension
    {
        public static IEnumerable<INavigatorAnimation> MatchNavigatorAnimationType(
            this IEnumerable<INavigatorAnimation> self,
            NavigatorAnimationType transitionType
            )
        {
            return self.Where(x => x.NavigatorAnimationType == transitionType);
        }

        public static IEnumerable<INavigatorAnimation> MatchFromScreenClassType(
            this IEnumerable<INavigatorAnimation> self,
            Type fromScreenClassType
        )
        {
            return self.Where(
                x => !string.IsNullOrEmpty(x.FromScreenName) &&
                     x.FromScreenName == fromScreenClassType.Name
            );
        }
        
        public static IEnumerable<INavigatorAnimation> MatchToScreenClassType(
            this IEnumerable<INavigatorAnimation> self,
            Type toScreenClassType
        )
        {
            return self.Where(
                x => !string.IsNullOrEmpty(x.ToScreenName) &&
                     x.ToScreenName == toScreenClassType.Name
            );
        }
    }
}