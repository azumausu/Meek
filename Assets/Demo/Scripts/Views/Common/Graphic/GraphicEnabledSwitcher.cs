using System;
using UnityEngine;
using UnityEngine.UI;

namespace Demo
{
    [RequireComponent(typeof(Graphic))]
    public class GraphicEnabledSwitcher : MonoBehaviour
    {
        private Graphic _graphic;
        
        private void Awake()
        {
            _graphic = GetComponent<Graphic>();
        }
        
        public void Switch(bool value)
        {
            _graphic.enabled = value;
        }
    }
}