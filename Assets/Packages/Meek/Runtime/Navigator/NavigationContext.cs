using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Meek
{
    public class NavigationContext
    {
        public IDictionary<string, object> Features;

        /// <summary>
        /// ナビゲーション前にアクティブだったScreen
        /// 注意: RemoveやInsertの場合はStackのPeekのScreenになります。
        /// 注意: Push前にStackが空の場合はnullになります。
        /// </summary>
        [CanBeNull] public IScreen FromScreen;

        /// <summary>
        /// ナビゲーション後にアクティブになるScreen
        /// 注意: RemoveやInsertの場合はStackのPeekのScreenになります。
        /// 注意: Popの後にStackが空になる場合はnullになります。
        /// </summary>
        [CanBeNull] public IScreen ToScreen;

        public IServiceProvider AppServices;
    }
}