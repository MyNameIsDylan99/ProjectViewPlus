using System.Collections.Generic;

namespace ProjectViewPlus
{
    /// <summary>
    /// All extension methods used in PVP
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Move an item from it's old index to the new index.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="item"></param>
        /// <param name="newIndex"></param>
        public static void Move<T>(this List<T> list, T item, int newIndex)
        {
            if (item != null)
            {
                var oldIndex = list.IndexOf(item);
                if (oldIndex > -1)
                {
                    list.RemoveAt(oldIndex);

                    if (newIndex > oldIndex) newIndex--;
                    // the actual index could have shifted due to the removal

                    list.Insert(newIndex, item);
                }
            }
        }
    }
}