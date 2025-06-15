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

        public PrefabViewHandler(IPrefabViewManager prefabViewManager, GameObject prefab)
        {
            var rootNode = new GameObject(prefab.name) { transform = { parent = prefabViewManager.PrefabRootNode } };
            var rootNodeRectTransform = rootNode.AddComponent<RectTransform>();
            rootNodeRectTransform.anchoredPosition3D = Vector3.zero;
            rootNodeRectTransform.anchorMin = Vector2.zero;
            rootNodeRectTransform.anchorMax = Vector2.one;
            rootNodeRectTransform.sizeDelta = Vector2.zero;
            rootNodeRectTransform.localScale = Vector3.one;
            var rootNodeCanvas = rootNode.AddComponent<Canvas>();
            rootNodeCanvas.overrideSorting = false;
            var rootNodeCanvasGroup = rootNode.AddComponent<CanvasGroup>();
            var rootNodeRaycaster = rootNode.AddComponent<GraphicRaycaster>();

            var instance = GameObject.Instantiate(prefab, rootNode.transform);
            instance.transform.SetParent(rootNode.transform);

            rootNode.SetLayerRecursively(prefabViewManager.PrefabRootNode.gameObject.layer);

            Instance = instance;
            RootNode = rootNode.transform;
            RectTransform = rootNodeRectTransform;
            Canvas = rootNodeCanvas;
            CanvasGroup = rootNodeCanvasGroup;
            NavigatorAnimationPlayer = instance.GetComponent<NavigatorAnimationPlayer>();
            GraphicRaycasters.Add(rootNodeRaycaster);
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

        protected virtual void Setup()
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

        void IViewHandler.Setup()
        {
            Setup();
        }

        void IViewHandler.EvaluateNavigateAnimation(
            NavigatorAnimationType animationType,
            Type fromScreenClassType,
            Type toScreenClassType,
            float t
        )
        {
            if (NavigatorAnimationPlayer == null) return;

            NavigatorAnimationPlayer.Evaluate(animationType, fromScreenClassType, toScreenClassType, Mathf.Clamp01(t));
        }

        IEnumerator IViewHandler.PlayNavigateAnimationRoutine(
            NavigatorAnimationType animationType,
            Type fromScreenClassType,
            Type toScreenClassType
        )
        {
            if (NavigatorAnimationPlayer == null) yield break;

            yield return animationType switch
            {
                NavigatorAnimationType.Open => NavigatorAnimationPlayer.OpenRoutine(fromScreenClassType,
                    toScreenClassType),
                NavigatorAnimationType.Close => NavigatorAnimationPlayer.CloseRoutine(fromScreenClassType,
                    toScreenClassType),
                NavigatorAnimationType.Show => NavigatorAnimationPlayer.ShowRoutine(fromScreenClassType,
                    toScreenClassType),
                NavigatorAnimationType.Hide => NavigatorAnimationPlayer.HideRoutine(fromScreenClassType,
                    toScreenClassType),
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