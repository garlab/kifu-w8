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
    }
}
