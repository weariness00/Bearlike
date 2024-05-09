using System;

namespace Util
{
    public static class EnumExtension
    {
        public static T Next<T>(this T value) where T : Enum
        {
            var values = (T[])Enum.GetValues(value.GetType());
            int nextIndex = (Array.IndexOf(values, value) + 1) % values.Length;
            return values[nextIndex];
        }
    }
}