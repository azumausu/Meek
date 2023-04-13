using System.Linq;
using Meek.NavigationStack;
using UnityEditor;
using UnityEngine;

namespace Meek.UGUI.Editor
{
    [CustomEditor(typeof(NavigatorAnimationPlayer))]
    public class NavigatorAnimationPlayerInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var transition = target as NavigatorAnimationPlayer;
            // DrawHowToUse();
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                DrawITransitionHandler(transition);
            }
            serializedObject.ApplyModifiedProperties();
        }

        void DrawHowToUse()
        {
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                EditorGUILayout.LabelField("使い方");
                using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                {
                    EditorGUILayout.LabelField("SimpleAnimationController");
                    using (new EditorGUI.IndentLevelScope())
                    {
                        EditorGUILayout.LabelField("SimpleAnimationControllerをこのノードにアタッチ");
                        EditorGUILayout.LabelField("open, close, show, hideの内使用したいものを設定");
                    }
                }
                using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                {
                    EditorGUILayout.LabelField("ITransitionHandler");
                    using (new EditorGUI.IndentLevelScope())
                    {
                        EditorGUILayout.LabelField("ITransitionHandlerを実装したComponentを子以下にアタッチ");
                    }
                }
            }
        }

        void DrawITransitionHandler(NavigatorAnimationPlayer t)
        {
            EditorGUILayout.LabelField("遷移アニメーションがアタッチされているGameObject");
            var handlers = t.GetComponentsInChildren<INavigatorAnimation>();
            if (handlers == null || handlers.Length == 0)
            {
                EditorGUILayout.LabelField("未使用");
                return;
            }

            var opens = handlers.Where(
                x => x.NavigatorAnimationType == NavigatorAnimationType.Open
            ).ToArray();
            var closes = handlers.Where(
                x => x.NavigatorAnimationType == NavigatorAnimationType.Close
            ).ToArray();
            var shows = handlers.Where(
                x => x.NavigatorAnimationType == NavigatorAnimationType.Show
            ).ToArray();
            var hides = handlers.Where(
                x => x.NavigatorAnimationType == NavigatorAnimationType.Hide
            ).ToArray();
            
            using (new EditorGUI.IndentLevelScope())
            {
                using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                {
                    using (new EditorGUI.DisabledGroupScope(true))
                    {
                        EditorGUILayout.LabelField("open");
                        if (opens.Length > 0)
                        {
                            using (new EditorGUILayout.VerticalScope())
                            {
                                foreach (var handler in opens)
                                {
                                    var mono = handler as MonoBehaviour;
                                    EditorGUILayout.ObjectField(mono.gameObject, typeof(GameObject), true);
                                }
                            }
                        }
                        else EditorGUILayout.LabelField("未使用");
                    }
                    using (new EditorGUI.DisabledGroupScope(true))
                    {
                        EditorGUILayout.LabelField("close");
                        if (closes.Length > 0)
                        {
                            using (new EditorGUILayout.VerticalScope())
                            {
                                foreach (var handler in closes)
                                {
                                    var mono = handler as MonoBehaviour;
                                    EditorGUILayout.ObjectField(mono.gameObject, typeof(GameObject), true);
                                }
                            }
                        }
                        else EditorGUILayout.LabelField("未使用");
                    }
                    using (new EditorGUI.DisabledGroupScope(true))
                    {
                        EditorGUILayout.LabelField("show");
                        if (shows.Length > 0)
                        {
                            using (new EditorGUILayout.VerticalScope())
                            {
                                foreach (var handler in shows)
                                {
                                    var mono = handler as MonoBehaviour;
                                    EditorGUILayout.ObjectField(mono.gameObject, typeof(GameObject), true);
                                }
                            }
                        }
                        else EditorGUILayout.LabelField("未使用");
                    }
                    using (new EditorGUI.DisabledGroupScope(true))
                    {
                        EditorGUILayout.LabelField("hides");
                        if (hides.Length > 0)
                        {
                            using (new EditorGUILayout.VerticalScope())
                            {
                                foreach (var handler in hides)
                                {
                                    var mono = handler as MonoBehaviour;
                                    EditorGUILayout.ObjectField(mono.gameObject, typeof(GameObject), true);
                                }
                            }
                        }
                        else EditorGUILayout.LabelField("未使用");
                    }
                }
            }
        }
    }
}
