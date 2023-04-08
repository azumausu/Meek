using System;
using System.Linq;
using Meek;
using Meek.NavigationStack;
using Meek.UGUI;
using UnityEngine;
using UnityEngine.UI;

namespace Sample
{
    public class UIManager : MonoBehaviourSingleton<UIManager>, IInputLocker, IPrefabViewManager
    {
        [SerializeField] private Image _inputBlocker;
        [SerializeField] private Transform _rootNode;

        /// <summary>
        /// Inputをロックする
        /// </summary>
        IDisposable IInputLocker.LockInput()
        {
            _inputBlocker.enabled = true;
            return new Disposer(() => _inputBlocker.enabled = false);
        }
        
        public bool IsInputLocking => _inputBlocker.enabled;

        void IPrefabViewManager.AddInHierarchy(PrefabViewHandler handler)
        {
            handler.RootNode.gameObject.SetLayerRecursively(_rootNode.gameObject.layer);
            handler.RootNode.SetParent(_rootNode);
        }

        void IPrefabViewManager.SortOrderInHierarchy(NavigationContext navigationContext)
        {
            var screenNavigator = navigationContext.AppServices.GetService<IScreenContainer>();
            var uis = screenNavigator.Screens.OfType<StackScreen>().Select(x => x.UI);
            foreach (var ui in uis)
            {
                foreach (var prefabView in ui.ViewHandlers.Reverse().OfType<PrefabViewHandler>())
                    prefabView.RootNode.SetAsFirstSibling();
            }
        }
    }
}