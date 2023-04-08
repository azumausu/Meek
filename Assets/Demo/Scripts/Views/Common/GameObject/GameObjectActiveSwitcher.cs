using UnityEngine;

namespace Demo
{
    public class GameObjectActiveSwitcher : MonoBehaviour
    {
        public void Switch(bool isActive)
        {
            gameObject.SetActive(isActive);
        }
    }
}