using System.Linq;
using JetBrains.Annotations;
using Meek.NavigationStack;
using Meek.UGUI;
using UnityEngine;

namespace Meek.MVP
{
    public static class ScreenUIExtension
    {
        /// <summary>
        ///     このSceneが持っているUIPrefabからT型のPresenterを取得します。
        /// </summary>
        [CanBeNull]
        public static T FindPresenter<T>(this ScreenUI self) where T : MonoBehaviour, IPresenter
        {
            foreach (var handler in self.ViewHandlers)
            {
                if (handler is not PrefabViewHandler prefabViewHandler) continue;

                var presenter = prefabViewHandler.Instance.GetComponent<T>();
                if (presenter != null) return presenter;
            }

            return null;
        }
    }
}