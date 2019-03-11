using System;
using System.Collections.Generic;
using System.Text;

namespace Jtfer.Ecp.Internals
{
    public static class EcpHelpers
    {
#if NET_4_6 || NET_STANDARD_2_0
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static int GetPowerOfTwoSize(int n)
        {
            if (n < 2)
            {
                return 2;
            }
            n--;
            n = n | (n >> 1);
            n = n | (n >> 2);
            n = n | (n >> 4);
            n = n | (n >> 8);
            n = n | (n >> 16);
            return n + 1;
        }

        /// Unique component pools. Dont change manually!
        public static readonly Dictionary<int, IComponentPool> ComponentPools = new Dictionary<int, IComponentPool>(512);

        /// <summary>
        /// Unique components count. Dont change manually!
        /// </summary>
        public static int ComponentPoolsCount;

        /// <summary>
        /// Unique pipeline contexts count. Dont change manually!
        /// </summary>
        public static int ContextCount;

        /// <summary>
        /// Unique entity type count. Dont change manually!
        /// </summary>
        public static int EntityTypesCount;
    }
}
