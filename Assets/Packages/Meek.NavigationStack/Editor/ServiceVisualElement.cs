using UnityEngine.UIElements;

namespace Meek.NavigationStack.Editor
{
    public class ServiceVisualElement : VisualElement
    {
        public ServiceVisualElement()
        {
            Add(new ListView());
        }
    }
}