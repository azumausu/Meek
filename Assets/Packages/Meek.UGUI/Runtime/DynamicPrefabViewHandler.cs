#if MEEK_ENABLE_UGUI
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Meek.NavigationStack;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Meek.UGUI
{
    public class DynamicPrefabViewHandler : IPrefabViewHandler
    {
        private IPrefabViewProvider _prefabViewProvider;
        protected readonly IPrefabViewManager PrefabViewManager;

        protected RectTransform RectTransform;
        protected CanvasGroup CanvasGroup;
        protected Canvas Canvas;
        protected NavigatorAnimationPlayer NavigatorAnimationPlayer;

        public readonly HashSet<GraphicRaycaster> GraphicRaycasters = new HashSet<GraphicRaycaster>();
        public readonly HashSet<IVisibilitySwitcher> VisibilitySwitchers = new HashSet<IVisibilitySwitcher>();
        public readonly HashSet<IInputSwitcher> InputSwitchers = new HashSet<IInputSwitcher>();

        public GameObject Instance { get; private set; }
        public Transform RootNode { get; private set; }

        public DynamicPrefabViewHandler(IPrefabViewManager prefabViewManager)
        {
            PrefabViewManager = prefabViewManager;
        }

        public virtual async ValueTask InitializeAsync(IPrefabViewProvider viewProvider, IScreen ownerScreen, [CanBeNull] object param)
        {
            _prefabViewProvider = viewProvider;

            var prefab = await viewProvider.ProvideAsync(ownerScreen, param);
            var parent = PrefabViewManager.GetRootNode(ownerScreen, param);
            var rootNode = new GameObject(prefab.name) { layer = parent.gameObject.layer, transform = { parent = parent } };

            var rootNodeRectTransform = rootNode.AddComponent<RectTransform>();
            rootNodeRectTransform.anchoredPosition3D = Vector3.zero;
            rootNodeRectTransform.anchorMin = Vector2.zero;
            rootNodeRectTransform.anchorMax = Vector2.one;
            rootNodeRectTransform.sizeDelta = Vector2.zero;
            rootNodeRectTransform.localScale = Vector3.one;

            var rootNodeCanvas = rootNode.AddComponent<Canvas>();
            rootNodeCanvas.overrideSorting = false;
            rootNode.AddComponent<CanvasGroup>();
            rootNode.AddComponent<GraphicRaycaster>();

            var instance = GameObject.Instantiate(prefab, rootNode.transform);


            Instance = instance;
            RootNode = rootNode.transform;
            RectTransform = rootNode.GetComponent<RectTransform>();
            Canvas = rootNode.GetComponent<Canvas>();
            CanvasGroup = rootNode.GetComponent<CanvasGroup>();
            NavigatorAnimationPlayer = instance.GetComponent<NavigatorAnimationPlayer>();

            var graphicRaycaster = rootNode.GetComponent<GraphicRaycaster>();
            if (graphicRaycaster != null)
            {
                GraphicRaycasters.Add(graphicRaycaster);
            }

            SetInteractable(false);
            SetVisibility(false);
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
            var disposable = Instance.GetComponent<IDisposable>();
            if (disposable != null)
            {
                try
                {
                    disposable.Dispose();
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }

            if (_prefabViewProvider is IDisposable disposableProvider)
            {
                try
                {
                    disposableProvider.Dispose();
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }

            if (RootNode != null && RootNode.gameObject != null)
            {
                Object.Destroy(RootNode.gameObject);
            }
        }

        public virtual async ValueTask DisposeAsync()
        {
            var asyncDisposable = Instance.GetComponent<IAsyncDisposable>();
            if (asyncDisposable != null)
            {
                try
                {
                    await asyncDisposable.DisposeAsync();
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }

            if (_prefabViewProvider is IAsyncDisposable asyncDisposableProvider)
            {
                try
                {
                    await asyncDisposableProvider.DisposeAsync();
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
        }

        #endregion
    }
}
#endif