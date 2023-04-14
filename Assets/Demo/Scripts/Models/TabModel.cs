using System;
using UniRx;

namespace Demo
{
    public class TabModel
    {
        private readonly ReactiveProperty<TabType> _selectingTab = new();
        public IServiceProvider AppServices { get; }
        
        public IReadOnlyReactiveProperty<TabType> SelectingTab => _selectingTab;

        public TabModel(IServiceProvider serviceProvider)
        {
            AppServices = serviceProvider;
        }
        
        public void UpdateTab(TabType tabType)
        {
            _selectingTab.Value = tabType;
        }
    }
}