namespace Meek.NavigationStack
{
    public enum NavigatorAnimationType
    {
        /// <summary>
        /// UI生成時
        /// </summary>
        Open,
        
        /// <summary>
        /// UI破棄時
        /// </summary>
        Close,
        
        /// <summary>
        /// Pause復帰時
        /// </summary>
        Show,
        
        /// <summary>
        /// Pause時
        /// </summary>
        Hide,
    }
}