using System.Collections.Generic;

namespace imnChecker
{
    static class ExtensionMethods
    {
        public static TItem GetSafe<TItem>(this IList<TItem> list, int index)
        {
            if (index < 0 || index >= list.Count)
            {
                return default(TItem);
            }
            return list[index];
        }

        public static string FindBetween(this string source, string start, string end)
        {
            int startI = source.IndexOf(start);
            if (startI == -1) return "";

            string ret = source.Substring(startI, source.IndexOf(end, startI + start.Length) + end.Length - startI).Replace(start, "").Replace(end, "");

            return ret;
        }
    }
}