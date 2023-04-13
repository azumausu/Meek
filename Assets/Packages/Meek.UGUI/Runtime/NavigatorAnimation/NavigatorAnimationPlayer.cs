using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Meek.NavigationStack;
using UnityEngine;

namespace Meek.UGUI
{
    public class NavigatorAnimationPlayer : MonoBehaviour
    {
        #region Fields

        private INavigatorAnimation[] _animationHandlers = new INavigatorAnimation[0];

        #endregion

        #region Methods

        public IEnumerator OpenRoutine(Type fromScreenClassType, Type toScreenClassType) =>
            PlayTransitionAnimation(
                NavigatorAnimationType.Open,
                fromScreenClassType,
                toScreenClassType
            );

        public IEnumerator CloseRoutine(Type fromScreenClassType, Type toScreenClassType) =>
            PlayTransitionAnimation(
                NavigatorAnimationType.Close,
                fromScreenClassType,
                toScreenClassType
            );

        public IEnumerator ShowRoutine(Type fromScreenClassType, Type toScreenClassType) =>
            PlayTransitionAnimation(
                NavigatorAnimationType.Show,
                fromScreenClassType,
                toScreenClassType
            );

        public IEnumerator HideRoutine(Type fromScreenClassType, Type toScreenClassType) =>
            PlayTransitionAnimation(
                NavigatorAnimationType.Hide,
                fromScreenClassType,
                toScreenClassType
            );

        /// <summary>
        /// 遷移アニメーションの開始状態に表示を変更する
        /// </summary>
        public void Evaluate(NavigatorAnimationType navigatorAnimationType, Type fromScreenClassType, Type toScreenClassType, float t)
        {
            var handlers = _animationHandlers
                .MatchNavigatorAnimationType(navigatorAnimationType)
                .ToArray();

            var allConditionMatchHandlers = handlers
                .MatchFromScreenClassType(fromScreenClassType)
                .MatchToScreenClassType(toScreenClassType)
                .ToArray();
            if (allConditionMatchHandlers.Length > 0)
            {
                Evaluate(allConditionMatchHandlers, t);
                return;
            }

            var fromScreenMatchHandlers = handlers
                .MatchFromScreenClassType(fromScreenClassType)
                .ToArray();
            if (fromScreenMatchHandlers.Length > 0)
            {
                Evaluate(fromScreenMatchHandlers, t);
                return;
            }

            var toScreenMatchHandlers = handlers
                .MatchToScreenClassType(toScreenClassType)
                .ToArray();
            if (toScreenMatchHandlers.Length > 0)
            {
                Evaluate(toScreenMatchHandlers, t);
                return;
            }
            
            var defaultHandlers = handlers
                .Where(x => string.IsNullOrEmpty(x.FromScreenName))
                .Where(x => string.IsNullOrEmpty(x.ToScreenName))
                .ToArray();
            if (defaultHandlers.Length > 0) Evaluate(defaultHandlers, t);
        }

        private IEnumerator PlayTransitionAnimation(NavigatorAnimationType transitionType, Type fromScreenClassType, Type toScreenClassType)
        {
            var handlers = _animationHandlers
                .MatchNavigatorAnimationType(transitionType)
                .ToArray();

            // 全ての条件に一致しているものがある場合はその遷移アニメーションを再生
            var allConditionMatchHandlers = handlers
                .MatchFromScreenClassType(fromScreenClassType)
                .MatchToScreenClassType(toScreenClassType)
                .ToArray();
            if (allConditionMatchHandlers.Length > 0)
            {
                yield return PlayAnimation(allConditionMatchHandlers);
                yield break;
            }

            // 遷移前のScreenの条件が一致してる遷移アニメーションを再生
            var fromScreenMatchHandlers = handlers
                .MatchFromScreenClassType(fromScreenClassType)
                .ToArray();
            if (fromScreenMatchHandlers.Length > 0)
            {
                yield return PlayAnimation(fromScreenMatchHandlers);
                yield break;
            }

            // 遷移先のScreenの条件が一致している遷移アニメーションを再生
            var toScreenMatchHandlers = handlers
                .MatchToScreenClassType(toScreenClassType)
                .ToArray();
            if (toScreenMatchHandlers.Length > 0)
            {
                yield return PlayAnimation(toScreenMatchHandlers);
                yield break;
            }
            
            // 条件なしの遷移アニメーションの再生
            var defaultHandlers = handlers
                .Where(x => string.IsNullOrEmpty(x.FromScreenName))
                .Where(x => string.IsNullOrEmpty(x.ToScreenName))
                .ToArray();
            if (defaultHandlers.Length > 0) yield return PlayAnimation(defaultHandlers);
        }

        private IEnumerator PlayAnimation(INavigatorAnimation[] handlers)
        {
            var count = handlers.Length;
            foreach (var handler in handlers) handler.Play(() => count--);

            yield return new WaitUntil(() => count == 0);
        }
        
        private void Evaluate(IEnumerable<INavigatorAnimation> handlers, float t)
        {
            foreach (var handler in handlers) handler.Evaluate(t);
        }

        #endregion

        #region Unity events

        private void Awake() => _animationHandlers =
            GetComponentsInChildren<INavigatorAnimation>();

        #endregion
    }
}