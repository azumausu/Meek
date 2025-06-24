#if MEEK_ENABLE_UGUI
using UnityEngine;

namespace Meek.UGUI
{
    public class DefaultPrefabViewManager : MonoBehaviour, IPrefabViewManager
    {
        [SerializeField] private RectTransform _rootNode;

        public Transform PrefabRootNode => _rootNode;
    }
}
#endif