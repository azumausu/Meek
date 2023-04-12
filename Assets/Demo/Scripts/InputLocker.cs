using System;
using Meek.NavigationStack;
using UnityEngine;
using UnityEngine.UI;

namespace Demo
{
    public class InputLocker : MonoBehaviour, IInputLocker
    {
        [SerializeField] private Image _inputBlocker;
        
        public IDisposable LockInput()
        {
            _inputBlocker.enabled = true;
            return new Disposer(() => _inputBlocker.enabled = false);
        }
        
        public bool IsInputLocking => _inputBlocker.enabled;
    }
}