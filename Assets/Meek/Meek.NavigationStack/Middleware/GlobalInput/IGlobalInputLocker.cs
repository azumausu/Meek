using System;

namespace Meek.NavigationStack
{
    /// <summary>
    /// 遷移処理中に全てのInputをロックするための機能を提供するインターフェース
    /// </summary>
    public interface IGlobalInputLocker
    {
        /// <summary>
        /// Inputをロックする
        /// </summary>
        IDisposable LockInput();
    
        public bool IsInputLocking { get; }
    }
}