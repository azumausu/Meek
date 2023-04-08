using System.Linq;
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
        public static T FindPresenter<T>(this ScreenUI self) where T : MonoBehaviour, IPresenter
        {
            return self.ViewHandlers
                .OfType<PrefabViewHandler>()
                .Select(ui => ui.Instance.GetComponent<T>())
                .FirstOrDefault(target => target != null);
        }

    }
}