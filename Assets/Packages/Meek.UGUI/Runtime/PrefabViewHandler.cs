#if MEEK_ENABLE_UGUI
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Meek.NavigationStack;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Meek.UGUI
{
    public class PrefabViewHandler : IViewHandler
    {
        protected readonly RectTransform RectTransform;
        protected readonly CanvasGroup CanvasGroup;
        protected readonly Canvas Canvas;
        protected readonly NavigatorAnimationPlayer NavigatorAnimationPlayer;

        public readonly HashSet<GraphicRaycaster> GraphicRaycasters = new HashSet<GraphicRaycaster>();
        public readonly HashSet<IVisibilitySwitcher> VisibilitySwitchers = new HashSet<IVisibilitySwitcher>();
        public readonly HashSet<IInputSwitcher> InputSwitchers = new HashSet<IInputSwitcher>();

        public GameObject Instance { get; private set; }
        public Transform RootNode { get; private set; }

        public PrefabViewHandler(Transform instanceRootNode, GameObject instance)
        {
            Instance = instance;
            RootNode = instanceRootNode;
            RectTransform = instanceRootNode.GetComponent<RectTransform>();
            Canvas = instanceRootNode.GetComponent<Canvas>();
            CanvasGroup = instanceRootNode.GetComponent<CanvasGroup>();
            NavigatorAnimationPlayer = instance.GetComponent<NavigatorAnimationPlayer>();

            var graphicRaycaster = instanceRootNode.GetComponent<GraphicRaycaster>();
            if (graphicRaycaster != null)
            {
                GraphicRaycasters.Add(graphicRaycaster);
            }
        }

        public virtual void SetInteractable(bool interactable)
        {
            if (CanvasGroup != null)
            {
                // Interactableを制御するとボタンの色が変わってしまう可能性があるので、
                // Raycastを取らないようするだけにする。
                CanvasGroup.blocksRaycasts = interactable;
            }

            foreach (var input in InputSwitchers)
            {
                if (interactable) input.Enable();
                else input.Disable();
            }
        }

        public virtual void SetVisibility(bool visible)
        {
            if (CanvasGroup != null) CanvasGroup.alpha = visible ? 1f : 0f;
            foreach (var visibility in VisibilitySwitchers)
            {
                if (visible) visibility.Show();
                else visibility.Hide();
            }
        }

        protected virtual void Setup(StackNavigationContext context)
        {
        }

        void IViewHandler.SetInteractable(bool interactable)
        {
            SetInteractable(interactable);
        }

        void IViewHandler.SetVisibility(bool visible)
        {
            SetVisibility(visible);
        }

        void IViewHandler.Setup(StackNavigationContext context)
        {
            Setup(context);
        }

        void IViewHandler.EvaluateNavigateAnimation(
            StackNavigationContext context,
            NavigatorAnimationType animationType,
            float t
        )
        {
            if (NavigatorAnimationPlayer == null) return;

            NavigatorAnimationPlayer.Evaluate(context, animationType, Mathf.Clamp01(t));
        }

        IEnumerator IViewHandler.PlayNavigateAnimationRoutine(StackNavigationContext context, NavigatorAnimationType animationType)
        {
            if (NavigatorAnimationPlayer == null) yield break;

            yield return animationType switch
            {
                NavigatorAnimationType.Open => NavigatorAnimationPlayer.OpenRoutine(context),
                NavigatorAnimationType.Close => NavigatorAnimationPlayer.CloseRoutine(context),
                NavigatorAnimationType.Show => NavigatorAnimationPlayer.ShowRoutine(context),
                NavigatorAnimationType.Hide => NavigatorAnimationPlayer.HideRoutine(context),
                _ => throw new ArgumentOutOfRangeException(nameof(animationType), animationType, null)
            };
        }

        #region IDisposable

        public virtual void Dispose()
        {
            if (RootNode != null && RootNode.gameObject != null)
            {
                Object.Destroy(RootNode.gameObject);
            }
        }

        public virtual ValueTask DisposeAsync()
        {
            return new ValueTask();
        }

        #endregion
    }
}
#endif