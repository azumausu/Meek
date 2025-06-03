using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

namespace Meek.NavigationStack.Editor
{
    public static class IStyleExtension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IStyle BorderWidth(this IStyle self, float value)
        {
            self.borderBottomWidth = value;
            self.borderRightWidth = value;
            self.borderLeftWidth = value;
            self.borderTopWidth = value;

            return self;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IStyle BorderColor(this IStyle self, Color color)
        {
            self.borderBottomColor = color;
            self.borderLeftColor = color;
            self.borderRightColor = color;
            self.borderTopColor = color;

            return self;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IStyle Margin(this IStyle self, float value)
        {
            self.marginBottom = value;
            self.marginRight = value;
            self.marginLeft = value;
            self.marginTop = value;

            return self;
        }
    }
}