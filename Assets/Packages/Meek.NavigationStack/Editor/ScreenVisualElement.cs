using UnityEditor.Graphs;
using UnityEngine;
using UnityEngine.UIElements;

namespace Meek.NavigationStack.Editor
{
    public class ScreenVisualElement : VisualElement
    {
        public ScreenVisualElement(string screenName)
        {
            this.style.justifyContent = new StyleEnum<Justify>(Justify.Center);
            this.style.backgroundColor = new StyleColor(new Color(0.0f, 1.0f, 0f, 0.1f));
            Add(new Label(screenName));
            Add(new Button()
            {
                text = "Remove",
            });
        }
    }
}