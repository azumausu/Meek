using System.Collections.Generic;
using JetBrains.Annotations;

namespace Meek.NavigationStack
{
    public class PushContext
    {
        public object NextScreenParameter = null;
        public bool IsCrossFade = false;
        public bool SkipAnimation = false;
        [CanBeNull] public Dictionary<string, object> CustomFeatures;
    }
}