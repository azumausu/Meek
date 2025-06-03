using UnityEditor.Graphs;
using UnityEngine;
using UnityEngine.UIElements;

namespace Meek.NavigationStack.Editor
{
    public class ScreenVisualElement : VisualElement
    {
        private readonly Label _screenNameLabel;
        private readonly Label _screenIndexLabel;

        public string screenName
        {
            get => _screenNameLabel.text;
            set => _screenNameLabel.text = value;
        }

        public int screenIndex
        {
            get => int.Parse(_screenIndexLabel.text);
            set => _screenIndexLabel.text = value.ToString();
        }

        public ScreenVisualElement()
        {
            style.Margin(5);
            style.backgroundColor = new Color(0, 0.5f, 0f, 0.1f);
            var root = new VisualElement()
            {
                style = { flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row), }
            };

            _screenIndexLabel = new Label()
            {
                style = { fontSize = 30, },
            };
            _screenNameLabel = new Label();
            var content = new VisualElement
            {
                style =
                {
                    flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row),
                    backgroundColor = new Color(0f, 0f, 0f, 0.1f),
                }
            };

            Add(_screenNameLabel);
            Add(root);

            root.Add(_screenIndexLabel);
            root.Add(content);
        }
    }
}