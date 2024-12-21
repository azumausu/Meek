#if MEEK_ENABLE_UGUI
using UnityEditor;
using UnityEngine;

namespace Meek.UGUI.Editor
{
    [CustomEditor(typeof(SimpleAnimationController))]
    public class SimpleAnimationControllerInspector : UnityEditor.Editor
    {
        SimpleAnimationController controller = null;

        void OnEnable()
        {
            controller = (SimpleAnimationController)target;
        }

        public override void OnInspectorGUI()
        {
            var entries = serializedObject.FindProperty("m_Entries");
            for (int i = 0; i < entries.arraySize; i++)
            {
                var e = entries.GetArrayElementAtIndex(i);
                var name = e.FindPropertyRelative("name");
                var clip = e.FindPropertyRelative("clip");
                var config = e.FindPropertyRelative("config");
                using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (EditorApplication.isPlaying)
                        {
                            if (GUILayout.Button("再生", GUILayout.ExpandWidth(false)))
                            {
                                controller.Play(name.stringValue);
                            }
                        }

                        name.stringValue = EditorGUILayout.TextField(name.stringValue);
                        clip.objectReferenceValue = EditorGUILayout.ObjectField(clip.objectReferenceValue, typeof(AnimationClip), false);

                        if (GUILayout.Button("削除", GUILayout.ExpandWidth(false)))
                        {
                            entries.DeleteArrayElementAtIndex(i);
                            serializedObject.ApplyModifiedProperties();
                            GUIUtility.ExitGUI();
                            break;
                        }
                    }
                    using (new EditorGUILayout.VerticalScope())
                    {
                        EditorGUILayout.PropertyField(config.FindPropertyRelative("FadeTime"), new GUIContent("補完時間"));
                        EditorGUILayout.PropertyField(config.FindPropertyRelative("PlaySpeed"), new GUIContent("再生速度"));
                        EditorGUILayout.PropertyField(config.FindPropertyRelative("StartPosition"), new GUIContent("アニメーション開始位置"));
                    }
                }
            }
            if (GUILayout.Button("追加"))
            {
                entries.arraySize++;

                // コピー元が0の場合は1に強制変更
                if (entries.arraySize > 0)
                {
                    var lastItem = entries.GetArrayElementAtIndex(entries.arraySize - 1);
                    if (lastItem != null)
                    {
                        if (lastItem.FindPropertyRelative("config").FindPropertyRelative("PlaySpeed").floatValue == 0f)
                        {
                            lastItem.FindPropertyRelative("config").FindPropertyRelative("PlaySpeed").floatValue = 1f;
                        }
                    }
                }
            }
            serializedObject.ApplyModifiedProperties();

            base.OnInspectorGUI();
        }
    }
}
#endif