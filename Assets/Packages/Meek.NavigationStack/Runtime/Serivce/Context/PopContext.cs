namespace Meek.NavigationStack
{
#nullable enable
    public class PopContext
    {
        public bool IsCrossFade = false;
        public bool SkipAnimation = false;
        public IScreen? OnlyWhenScreen = null;
        public bool UseOnlyWhenScreen => OnlyWhenScreen != null;
    }
}