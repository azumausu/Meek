#if MEEK_ENABLE_UGUI
using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Meek.NavigationStack;
using UnityEngine;
using UnityEngine.Pool;

namespace Meek.UGUI
{
    public class NavigatorAnimationPlayer : MonoBehaviour
    {
        #region Fields

        private static readonly Dictionary<Type, string> ScreenTypeToNameCache = new();
        private INavigatorAnimation[] _animationHandlers;

        #endregion

        #region Methods

        public IEnumerator OpenRoutine(StackNavigationContext context)
        {
            return PlayTransitionAnimation(context, NavigatorAnimationType.Open);
        }

        public IEnumerator CloseRoutine(StackNavigationContext context)
        {
            return PlayTransitionAnimation(context, NavigatorAnimationType.Close);
        }

        public IEnumerator ShowRoutine(StackNavigationContext context)
        {
            return PlayTransitionAnimation(context, NavigatorAnimationType.Show);
        }

        public IEnumerator HideRoutine(StackNavigationContext context)
        {
            return PlayTransitionAnimation(context, NavigatorAnimationType.Hide);
        }

        /// <summary>
        /// 遷移アニメーションの開始状態に表示を変更する
        /// </summary>
        public void Evaluate(StackNavigationContext context, NavigatorAnimationType navigatorAnimationType, float t)
        {
            using var disposable1 = ListPool<INavigatorAnimation>.Get(out var targetHandlers);
            using var disposable2 = ListPool<INavigatorAnimation>.Get(out var conditionMatchHandlers);

            var fromScreenName = GetScreenTypeName(context.FromScreen);
            var toScreenName = GetScreenTypeName(context.ToScreen);

            foreach (var handler in _animationHandlers)
            {
                if (handler.IsMatchNavigatorAnimationType(navigatorAnimationType))
                {
                    targetHandlers.Add(handler);
                }
            }

            // === 条件に完全一致するハンドラを探す ===
            foreach (var handler in targetHandlers)
            {
                if (handler.IsMatchFromScreenName(fromScreenName) && handler.IsMatchToScreenName(toScreenName))
                {
                    conditionMatchHandlers.Add(handler);
                }
            }

            if (conditionMatchHandlers.Count > 0)
            {
                Evaluate(conditionMatchHandlers, t);
                return;
            }

            // === 遷移元のScreen名に一致するハンドラを探す ===
            foreach (var handler in targetHandlers)
            {
                if (handler.IsMatchFromScreenName(fromScreenName) && string.IsNullOrEmpty(handler.ToScreenName))
                {
                    conditionMatchHandlers.Add(handler);
                }
            }

            if (conditionMatchHandlers.Count > 0)
            {
                Evaluate(conditionMatchHandlers, t);
                return;
            }

            // === 遷移先のScreen名に一致するハンドラを探す ===
            foreach (var handler in targetHandlers)
            {
                if (handler.IsMatchToScreenName(toScreenName) && string.IsNullOrEmpty(handler.FromScreenName))
                {
                    conditionMatchHandlers.Add(handler);
                }
            }

            if (conditionMatchHandlers.Count > 0)
            {
                Evaluate(conditionMatchHandlers, t);
                return;
            }

            // === デフォルトハンドラを探す ===
            foreach (var handler in targetHandlers)
            {
                if (string.IsNullOrEmpty(handler.FromScreenName) && string.IsNullOrEmpty(handler.ToScreenName))
                {
                    conditionMatchHandlers.Add(handler);
                }
            }

            if (conditionMatchHandlers.Count > 0)
            {
                Evaluate(conditionMatchHandlers, t);
            }
        }

        private IEnumerator PlayTransitionAnimation(StackNavigationContext context, NavigatorAnimationType navigatorAnimationType)
        {
            using var disposable1 = ListPool<INavigatorAnimation>.Get(out var targetHandlers);
            using var disposable2 = ListPool<INavigatorAnimation>.Get(out var conditionMatchHandlers);

            var fromScreenName = GetScreenTypeName(context.FromScreen);
            var toScreenName = GetScreenTypeName(context.ToScreen);

            foreach (var handler in _animationHandlers)
            {
                if (handler.IsMatchNavigatorAnimationType(navigatorAnimationType))
                {
                    targetHandlers.Add(handler);
                }
            }

            // === 条件に完全一致するハンドラを探す ===
            foreach (var handler in targetHandlers)
            {
                if (handler.IsMatchFromScreenName(fromScreenName) && handler.IsMatchToScreenName(toScreenName))
                {
                    conditionMatchHandlers.Add(handler);
                }
            }

            if (conditionMatchHandlers.Count > 0)
            {
                yield return PlayAnimation(conditionMatchHandlers);
                yield break;
            }

            // === 遷移元のScreen名に一致するハンドラを探す ===
            foreach (var handler in targetHandlers)
            {
                if (handler.IsMatchFromScreenName(fromScreenName) && string.IsNullOrEmpty(handler.ToScreenName))
                {
                    conditionMatchHandlers.Add(handler);
                }
            }

            if (conditionMatchHandlers.Count > 0)
            {
                yield return PlayAnimation(conditionMatchHandlers);
                yield break;
            }

            // === 遷移先のScreen名に一致するハンドラを探す ===
            foreach (var handler in targetHandlers)
            {
                if (handler.IsMatchToScreenName(toScreenName) && string.IsNullOrEmpty(handler.FromScreenName))
                {
                    conditionMatchHandlers.Add(handler);
                }
            }

            if (conditionMatchHandlers.Count > 0)
            {
                yield return PlayAnimation(conditionMatchHandlers);
                yield break;
            }

            // === デフォルトハンドラを探す ===
            foreach (var handler in targetHandlers)
            {
                if (string.IsNullOrEmpty(handler.FromScreenName) && string.IsNullOrEmpty(handler.ToScreenName))
                {
                    conditionMatchHandlers.Add(handler);
                }
            }

            if (conditionMatchHandlers.Count > 0)
            {
                yield return PlayAnimation(conditionMatchHandlers);
                yield break;
            }
        }

        private IEnumerator PlayAnimation(List<INavigatorAnimation> handlers)
        {
            var count = handlers.Count;
            foreach (var handler in handlers) handler.Play(() => count--);

            while (count != 0)
            {
                yield return null;
            }
        }

        private void Evaluate(List<INavigatorAnimation> handlers, float t)
        {
            foreach (var handler in handlers) handler.Evaluate(t);
        }

        [CanBeNull]
        private string GetScreenTypeName([CanBeNull] IScreen screen)
        {
            if (screen == null) return null;

            var type = screen.GetType();
            if (ScreenTypeToNameCache.TryGetValue(type, out var screenName))
            {
                return screenName;
            }

            screenName = type.Name;
            ScreenTypeToNameCache[type] = screenName;
            return screenName;
        }

        #endregion

        #region Unity events

        private void Awake()
        {
            _animationHandlers = GetComponentsInChildren<INavigatorAnimation>();
        }

        #endregion
    }
}
#endif