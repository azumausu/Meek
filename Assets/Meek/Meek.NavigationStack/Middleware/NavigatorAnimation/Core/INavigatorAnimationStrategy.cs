using System.Collections;

namespace Meek.NavigationStack
{
    public interface INavigatorAnimationStrategy
    {
        /// <summary>
        /// trueの場合このAnimationを実行します
        /// </summary>
        bool IsValid(StackNavigationContext context);

        /// <summary>
        /// 遷移Animationの再生
        /// </summary>
        IEnumerator PlayAnimationRoutine(StackNavigationContext context);
    }
}