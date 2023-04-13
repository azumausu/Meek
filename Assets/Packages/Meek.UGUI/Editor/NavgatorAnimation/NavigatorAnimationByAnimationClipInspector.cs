using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Meek.UGUI.Editor
{
    [CustomEditor(typeof(NavigatorAnimationByAnimationClip))]
    public class NavigatorAnimationByAnimationClipInspector : UnityEditor.Editor
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
                    new GUIContent("Check FromStateType")
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
                    new GUIContent("Check ToStateType")
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
                .Where(x => x.GetInterfaces().Any(y => y == typeof(IScreen)))
                .Where(x => !x.IsAbstract)
                .Select(x => x.Name)
                .ToArray();

            _selectedFromScreenNameIndex = _screenTypeNames
                .Select((typeName, index) => new { TypeName = typeName, Index = index, })
                .FirstOrDefault(x => x.TypeName == _fromScreenNameProperty.stringValue)
                ?.Index ?? 0;
            _selectedToScreenNameIndex = _screenTypeNames
                .Select((typeName, index) => new { TypeName = typeName, Index = index, })
                .FirstOrDefault(x => x.TypeName == _toScreenNameProperty.stringValue)
                ?.Index ?? 0;
        }

        #endregion
    }
}