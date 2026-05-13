#if MEEK_ENABLE_UGUI
using System;
using UnityEngine;

namespace Meek.UGUI
{
    public class UIInputSyncer : MonoBehaviour, IInputSwitcher
    {
        private IInputSwitcher[] _inputSwitchers = Array.Empty<IInputSwitcher>();
        
        public void SyncInput(params IInputSwitcher[] inputSwitchers)
        {
            _inputSwitchers = inputSwitchers;
        }

        void IInputSwitcher.Enable()
        {
            foreach (var input in _inputSwitchers) input.Enable();
        }
        
        void IInputSwitcher.Disable()
        {
            foreach (var input in _inputSwitchers) input.Disable();
        }
    }
}
#endif