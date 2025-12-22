using System.Collections;
using UnityEngine;

namespace Meek.NavigationStack
{
    public class CoroutineRunner : ICoroutineRunner
    {
        private readonly GameObject _gameObject;
        private readonly CoroutineRunnerComponent _component;
        
        public CoroutineRunner()
        {
            _gameObject = new GameObject("CoroutineRunner");
            _component = _gameObject.AddComponent<CoroutineRunnerComponent>();
            GameObject.DontDestroyOnLoad(_gameObject);
        }
        
        public void Dispose()
        {
            GameObject.Destroy(_gameObject);
        }
        
        public Coroutine StartCoroutine(IEnumerator routine)
        {
            return _component.StartCoroutine(routine);
        }
    }
}