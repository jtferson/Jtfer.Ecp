using System;
using System.Collections.Generic;
using System.Text;

namespace Jtfer.Ecp
{
    public class EntityOwner
    {
        protected const int MinSize = 256;

        public int[] Entities =  new int[MinSize];
        internal protected int _entitiesCount;

        /// <summary>
        /// Will be raised by Domain for new compatible with this filter entity.
        /// Do not call it manually!
        /// </summary>
        /// <param name="entity">Entity id.</param>
#if NET_4_6 || NET_STANDARD_2_0
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public void RaiseOnAddEvent(int entity)
        {
            if (Entities.Length == _entitiesCount)
            {
                Array.Resize(ref Entities, _entitiesCount << 1);
            }
            Entities[_entitiesCount++] = entity;
        }

        /// <summary>
        /// Will be raised by Domain for old already non-compatible with this filter entity.
        /// Do not call it manually!
        /// </summary>
        /// <param name="entity">Entity id.</param>
#if NET_4_6 || NET_STANDARD_2_0
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public void RaiseOnRemoveEvent(int entity)
        {
            for (var i = 0; i < _entitiesCount; i++)
            {
                if (Entities[i] == entity)
                {
                    _entitiesCount--;
                    Array.Copy(Entities, i + 1, Entities, i, _entitiesCount - i);
                    break;
                }
            }
        }
    }
}
