using System.Collections.Generic;
using System.Linq;
using System;
using Meek.NavigationStack.Debugs;
using UnityEngine;
using UnityEngine.UIElements;

namespace Meek.NavigationStack.Editor
{
    public class ScreenContainerView : VisualElement
    {
        private ServiceEntry _serviceEntry;
        private IScreenContainer _screenContainer;
        private readonly List<string> _screenFullNames = new();
        private readonly Label _label;
        private readonly VisualElement _screenListView;
        private readonly ListView _listView;

        public ServiceEntry ServiceEntry
        {
            get => _serviceEntry;
            set
            {
                _serviceEntry = value;
                _screenContainer = value.ServiceProvider.GetService<IScreenContainer>();
                Refresh();
            }
        }

        public string ScreenContainerName
        {
            get => _label.text;
            set => _label.text = value;
        }

        public ScreenContainerView()
        {
            style.BorderColor(new Color(0f, 1f, 0f, 0.2f));
            style.backgroundColor = new Color(0f, 1f, 0f, 0.1f);
            style.BorderWidth(3);
            style.Margin(10);
            _label = new Label();
            _screenListView = new VisualElement();
            _listView = new ListView
            {
                itemsSource = _screenFullNames,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                makeItem = () => new ScreenVisualElement(),
                bindItem = (e, i) =>
                {
                    var screenVisualElement = e as ScreenVisualElement;
                    screenVisualElement.screenName = _screenFullNames[i];
                    screenVisualElement.screenIndex = i;
                }
            };

            Add(_label);
            Add(_screenListView);
        }

        public void Refresh()
        {
            if (_screenContainer == null) return;

            _screenFullNames.AddRange(_screenContainer.Screens
                .Reverse()
                .Select(x => x.GetType().FullName)
            );
            _screenListView.Clear();
            int no = 1;
            foreach (var screen in _screenContainer.Screens.Reverse())
            {
                var screenVisualElement = new ScreenVisualElement()
                {
                    screenName = screen.GetType().FullName,
                    screenIndex = no,
                };

                _screenListView.Add(screenVisualElement);
                no++;
            }
        }

        public bool Equal(IScreenContainer screenContainer)
        {
            return _screenContainer == screenContainer;
        }

        public bool Equal(ServiceEntry serviceEntry)
        {
            return _serviceEntry == serviceEntry;
        }

        public bool Equal(IServiceProvider serviceProvider)
        {
            return _serviceEntry.ServiceProvider == serviceProvider;
        }
    }
}