using System;
using System.Collections.Generic;

namespace Kifu.Utils
{
    public static class XLinq
    {
        public static void Each<T>(this IEnumerable<T> enumberable, Action<T> action)
        {
            foreach (var item in enumberable)
            {
                action(item);
            }
        }

        public static void Times(this int n, Action<int> action)
        {
            for (int i = 0; i < n; ++i)
            {
                action(i);
            }
        }
    }
}
