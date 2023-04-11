using System;
using UnityEngine;
using UnityEngine.UI;

namespace Demo
{
    [RequireComponent(typeof(RawImage))]
    public class ProductImage : MonoBehaviour
    {
        private RawImage _rawImage;

        private void Awake()
        {
            _rawImage = GetComponent<RawImage>();
        }

        public void UpdateView(int productId)
        {
            var texture = Resources.Load<Texture2D>($"ProductTextures/product_{productId:D3}");
            _rawImage.texture = texture;
        }
    }
}