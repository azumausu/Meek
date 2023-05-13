using System;
using System.Collections.Generic;
using System.Linq;
using Meek.NavigationStack.Debugs;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Meek.NavigationStack.Editor
{
    public class ScreenContainerWindow : EditorWindow
    {
        [MenuItem("Meek/NavigationStack/ScreenContainerWindow")]
        public static void Open()
        {
            var window = GetWindow<ScreenContainerWindow>();
            window.titleContent = new GUIContent("ScreenContainerWindow");
        }

        private void CreateGUI()
        {
            rootVisualElement.Add(new ScreenVisualElement("Screen1")
            {
            });
        }

        private void OnEnable()
        {
            RuntimeNavigationStackManager.Instance.OnRegisterServices += OnRegister;
            RuntimeNavigationStackManager.Instance.OnUnregisterServices += OnUnregister;

            var serviceEntry = RuntimeNavigationStackManager.Instance.ServiceEntries.FirstOrDefault();
            if (serviceEntry == null) return;

            var screenContainer = serviceEntry.ServiceProvider.GetService<IScreenContainer>();
            var itemList = screenContainer.Screens
                .Reverse()
                .Select(x => x.GetType().FullName)
                .ToList();
            var rootElement = rootVisualElement;
            rootElement.Add(new ListView()
            {
                itemsSource = itemList,
                fixedItemHeight = 50,
                makeItem = () => new Label(),
                bindItem = (e, i) =>
                {
                    var label = e as Label;
                    label.text = itemList[i];
                }
            });
        }

        private void OnDisable()
        {
            RuntimeNavigationStackManager.Instance.OnRegisterServices -= OnRegister;
            RuntimeNavigationStackManager.Instance.OnUnregisterServices -= OnUnregister;
        }

        private void OnRegister(ServiceEntry serviceEntry)
        {
        }

        private void OnUnregister(ServiceEntry serviceEntry)
        {
        }
    }
}