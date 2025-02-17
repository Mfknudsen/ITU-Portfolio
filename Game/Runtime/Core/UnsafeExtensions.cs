#region Libraries

using Unity.Collections.LowLevel.Unsafe;

#endregion

namespace Runtime.Core
{
    public static class UnsafeExtensions
    {
        #region UnsafeList

        public static bool ListContains<T>(this UnsafeList<T> list, T element) where T : unmanaged
        {
            foreach (T e in list)
                if (e.Equals(element))
                    return true;

            return false;
        }

        #endregion
    }
}