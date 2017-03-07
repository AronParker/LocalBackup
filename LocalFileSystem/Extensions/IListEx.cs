using System.Collections.Generic;

namespace LocalFileSystem.Extensions
{
    internal static class IListEx
    {
        public static void Swap<T>(this IList<T> list, int i, int j)
        {
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
}
