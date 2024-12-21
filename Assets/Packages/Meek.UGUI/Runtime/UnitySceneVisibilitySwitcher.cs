#if MEEK_ENABLE_UGUI
using UnityEngine;

namespace Meek.UGUI
{
    public class UnitySceneVisibilitySwitcher : MonoBehaviour, IVisibilitySwitcher
    {
        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
#endif