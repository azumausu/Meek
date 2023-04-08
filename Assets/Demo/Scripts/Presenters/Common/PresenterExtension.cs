using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Meek.MVP;
using UnityEngine;
using UnityEngine.SceneManagement;
using UniRx;

namespace Harmos.UI.UGUI.MVP
{
    public static class PresenterExtension
    {
        /// <summary>
        /// Scene名被りも考慮しつつ、AdditiveでSceneをロード
        /// また、表示/非表示をPresenterに同期する
        /// </summary>
        public static async Task<TComponent> LoadUnitySceneAsync<TComponent>(this IPresenter presenter, string sceneName)
            where TComponent : MonoBehaviour
        {
            var monoBehaviour = presenter as MonoBehaviour;
            TComponent targetComponent = null;
            
            // Sceneの生成・破棄処理実装
            var cts = new TaskCompletionSource<bool>();
            var currentSceneCount = SceneManager.sceneCount;
            var handler = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            handler.completed += _ => { cts.TrySetResult(true); };
            if (handler.isDone) cts.TrySetResult(true);
            await cts.Task;
            
            // 名前で検索すると同一Sceneを複数ロードしている場合に旧Sceneを取得してしまうので、
            // SceneCountで取得する
            var newScene = SceneManager.GetSceneAt(currentSceneCount);
            if (!newScene.name.Contains(sceneName))
                throw new ArgumentException("異なるSceneを取得しています。");

            // ロードしたSceneのRootGameObjectを一つにする。
            var rootGameObject = new GameObject("Root");
            SceneManager.MoveGameObjectToScene(rootGameObject, newScene);
            rootGameObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            rootGameObject.transform.localScale = Vector3.one;
            
            // Tコンポーネントを取得
            foreach (var go in newScene.GetRootGameObjects())
            {
                // InGameManagerを取得する
                if (targetComponent == null) targetComponent = go.GetComponentInChildren<TComponent>();
                go.transform.SetParent(rootGameObject.transform);
            }

            new Disposer(() =>
            {
                SceneManager.UnloadSceneAsync(newScene);
                Resources.UnloadUnusedAssets();
            }).AddTo(monoBehaviour);

            if (targetComponent == null) throw new ArgumentException($"型{typeof(TComponent)}のオブジェクトが存在しません。");

            SceneManager.SetActiveScene(newScene);
            return targetComponent;
        }
    }
}