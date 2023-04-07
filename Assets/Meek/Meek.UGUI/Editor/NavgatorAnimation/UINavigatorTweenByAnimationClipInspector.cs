using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Meek.UGUI.Editor
{
    [CustomEditor(typeof(UINavigatorTweenByAnimationClip))]
    public class UINavigatorTweenByAnimationClipInspector : UnityEditor.Editor
    {
        #region Fields

        private int _selectedFromScreenNameIndex;
        private int _selectedToScreenNameIndex;

        private string[] _screenTypeNames = new string[0];

        private SerializedProperty _enabledFromScreenNameProperty;
        private SerializedProperty _enabledToScreenNameProperty;
        private SerializedProperty _fromScreenNameProperty;
        private SerializedProperty _toScreenNameProperty;

        #endregion

        #region Methods

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                EditorGUILayout.PropertyField(
                    _enabledFromScreenNameProperty,
                    new GUIContent("遷移前のStateTypeを指定する。")
                );
                if (_enabledFromScreenNameProperty.boolValue)
                {
                    _selectedFromScreenNameIndex = EditorGUILayout.Popup(
                        _selectedFromScreenNameIndex,
                        _screenTypeNames
                    );
                    _fromScreenNameProperty.stringValue =
                        _screenTypeNames[_selectedFromScreenNameIndex];
                }

                EditorGUILayout.PropertyField(
                    _enabledToScreenNameProperty,
                    new GUIContent("遷移後のStateTypeを指定する。")
                );
                if (_enabledToScreenNameProperty.boolValue)
                {
                    _selectedToScreenNameIndex = EditorGUILayout.Popup(
                        _selectedToScreenNameIndex,
                        _screenTypeNames
                    );
                    _toScreenNameProperty.stringValue =
                        _screenTypeNames[_selectedToScreenNameIndex];
                }
            }

            serializedObject.ApplyModifiedProperties();
            Undo.RecordObject(this, name);
        }

        #endregion

        #region Unity events

        public void OnEnable()
        {
            _enabledFromScreenNameProperty = serializedObject.FindProperty(
                nameof(_enabledFromScreenNameProperty)
                    .Replace("Property", "")
            );
            _enabledToScreenNameProperty = serializedObject.FindProperty(
                nameof(_enabledToScreenNameProperty)
                    .Replace("Property", "")
            );
            _fromScreenNameProperty = serializedObject.FindProperty(
                nameof(_fromScreenNameProperty)
                    .Replace("Property", "")
            );
            _toScreenNameProperty = serializedObject.FindProperty(
                nameof(_toScreenNameProperty)
                    .Replace("Property", "")
            );


            _screenTypeNames = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(
                    x => x.IsSubclassOf(typeof(IScreen)) ||
                         x == typeof(IScreen)
                )
                .Select(x => x.Name)
                .ToArray();

            var gameStateIndex = _screenTypeNames.Select(
                    (typeName, index) => new
                    {
                        TypeName = typeName,
                        Index = index,
                    }
                )
                .First(x => x.TypeName == nameof(IScreen))
                .Index;

            _selectedFromScreenNameIndex = _screenTypeNames.Select(
                    (typeName, index) => new
                    {
                        TypeName = typeName,
                        Index = index,
                    }
                )
                .FirstOrDefault(
                    x => x.TypeName == _fromScreenNameProperty.stringValue
                )
                ?.Index ?? gameStateIndex;
            _selectedToScreenNameIndex = _screenTypeNames.Select(
                    (typeName, index) => new
                    {
                        TypeName = typeName,
                        Index = index,
                    }
                )
                .FirstOrDefault(
                    x => x.TypeName == _toScreenNameProperty.stringValue
                )
                ?.Index ?? gameStateIndex;
        }

        #endregion
    }
}