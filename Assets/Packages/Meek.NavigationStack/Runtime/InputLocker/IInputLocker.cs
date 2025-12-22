using System;

namespace Meek.NavigationStack
{
    /// <summary>
    /// 遷移処理中に全てのInputをロックするための機能を提供するインターフェース
    /// </summary>
    public interface IInputLocker
    {
        /// <summary>
        /// Inputをロックする
        /// </summary>
        IDisposable LockInput();
    
        public bool IsInputLocking { get; }
    }
}