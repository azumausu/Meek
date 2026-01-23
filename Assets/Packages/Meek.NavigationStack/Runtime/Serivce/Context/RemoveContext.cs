using System.Collections.Generic;
using JetBrains.Annotations;

namespace Meek.NavigationStack
{
    public class RemoveContext
    {
        public bool IsCrossFade = false;
        public bool SkipAnimation = true;
        [CanBeNull] public Dictionary<string, object> CustomFeatures;
    }
}