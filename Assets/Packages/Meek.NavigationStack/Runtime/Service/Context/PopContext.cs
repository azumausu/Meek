using System.Collections.Generic;
using JetBrains.Annotations;

namespace Meek.NavigationStack
{
#nullable enable
    public class PopContext
    {
        public bool IsCrossFade = false;
        public bool SkipAnimation = false;
        public IScreen? OnlyWhenScreen = null;
        public bool UseOnlyWhenScreen => OnlyWhenScreen != null;
        public Dictionary<string, object>? CustomFeatures;
    }
}