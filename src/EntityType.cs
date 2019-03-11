using System;
using System.Collections.Generic;
using System.Text;

namespace Jtfer.Ecp
{

    public interface IEntityType { }

    internal class EntityType<T>
        where T : IEntityType
    {
        public readonly int TypeIndex;
        public static readonly EntityType<T> Instance = new EntityType<T>();
        EntityType()
        {
            TypeIndex = Internals.EcpHelpers.EntityTypesCount++;
        }
    }
}
