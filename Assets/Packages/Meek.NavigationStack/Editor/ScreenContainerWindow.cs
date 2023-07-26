using System;
using System.Linq;
using Meek.NavigationStack.Debugs;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Meek.NavigationStack.Editor
{
    public class ScreenContainerWindow : EditorWindow
    {
        private ListView _rootListView;

        [MenuItem("Meek/NavigationStack/ScreenContainerWindow")]
        public static void Open()
        {
            var window = GetWindow<ScreenContainerWindow>();
            window.titleContent = new GUIContent("ScreenContainerWindow");
        }

        private void CreateGUI()
        {
            RuntimeNavigationStackManager.Instance.OnRegisterServices += OnRegister;
            RuntimeNavigationStackManager.Instance.OnUnregisterServices += OnUnregister;
            RuntimeNavigationStackManager.Instance.ScreenDidNavigate += ScreenDidNavigate;

            _rootListView = new ListView()
            {
                itemsSource = RuntimeNavigationStackManager.Instance.ServiceEntries,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                makeItem = () => new ScreenContainerView(),
                bindItem = (e, i) =>
                {
                    var screenContainerView = e as ScreenContainerView;
                    screenContainerView.ScreenContainerName = $"Screen Container - {i}";
                    screenContainerView.ServiceEntry = RuntimeNavigationStackManager.Instance.ServiceEntries[i];
                }
            };
            _rootListView.RefreshItems();
            rootVisualElement.Add(_rootListView);
            
        }

        private void OnDestroy()
        {
            RuntimeNavigationStackManager.Instance.OnRegisterServices -= OnRegister;
            RuntimeNavigationStackManager.Instance.OnUnregisterServices -= OnUnregister;
            RuntimeNavigationStackManager.Instance.ScreenDidNavigate -= ScreenDidNavigate;
        }

        private void OnRegister(ServiceEntry serviceEntry)
        {
            _rootListView.RefreshItems();
        }

        private void OnUnregister(ServiceEntry serviceEntry)
        {
            _rootListView.RefreshItems();
        }

        private void ScreenDidNavigate(StackNavigationContext context)
        {
            // https://forum.unity.com/threads/how-to-get-listview-child-item-visualelement-from-itemssource-index.821388/
            var view = _rootListView.Query<ScreenContainerView>()
                .ToList()
                .FirstOrDefault(x => x.Equal(context.AppServices));
            view?.Refresh();
            _rootListView.RefreshItems();
        }
    }
}