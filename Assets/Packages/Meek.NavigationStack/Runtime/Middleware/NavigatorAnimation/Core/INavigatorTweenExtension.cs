using System;
using System.Collections.Generic;
using System.Linq;

namespace Meek.NavigationStack
{
    public static class INavigatorTweenExtension
    {
        public static IEnumerable<INavigatorTween> MatchNavigatorAnimationType(
            this IEnumerable<INavigatorTween> self,
            NavigatorAnimationType transitionType
            )
        {
            return self.Where(x => x.NavigatorAnimationType == transitionType);
        }

        public static IEnumerable<INavigatorTween> MatchFromScreenClassType(
            this IEnumerable<INavigatorTween> self,
            Type fromScreenClassType
        )
        {
            return self.Where(
                x => !string.IsNullOrEmpty(x.FromScreenName) &&
                     x.FromScreenName == fromScreenClassType.Name
            );
        }
        
        public static IEnumerable<INavigatorTween> MatchToScreenClassType(
            this IEnumerable<INavigatorTween> self,
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