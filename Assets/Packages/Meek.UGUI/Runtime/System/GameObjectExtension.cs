using UnityEngine;

namespace Meek.UGUI
{
    internal static class GameObjectExtension
    {
        /// <summary>
        ///     自分自身を含むすべての子オブジェクトのレイヤーを設定します
        /// </summary>
        public static void SetLayerRecursively(this GameObject self, int layer)
        {
            self.layer = layer;

            foreach (Transform n in self.transform) SetLayerRecursively(n.gameObject, layer);
        }
    }
}