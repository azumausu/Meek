using UnityEngine;

namespace Meek.UGUI
{
    public class UIVisibilitySyncer : MonoBehaviour, IVisibilitySwitcher
    {
        private IVisibilitySwitcher[] _visibilitySwitchers;
        
        public void SyncVisibility(params IVisibilitySwitcher[] visibilitySwitchers)
        {
            _visibilitySwitchers = visibilitySwitchers;
        }

        void IVisibilitySwitcher.Show()
        {
            foreach (var visibility in _visibilitySwitchers) visibility.Show();
        }

        void IVisibilitySwitcher.Hide()
        {
            foreach (var visibility in _visibilitySwitchers) visibility.Hide(); 
        }
    }
}