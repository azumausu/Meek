namespace Meek.NavigationStack
{
    public enum ScreenUIType
    {
        /// <summary>
        /// UIが存在しないState
        /// </summary>
        None,
        
        /// <summary>
        /// UIが全画面表示されるState
        /// </summary>
        FullScreen,
        
        /// <summary>
        /// UIがWindowで表示されたり、下のUIが透過されるState
        /// </summary>
        WindowOrTransparent,
    }
}
