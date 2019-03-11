using System;
using System.Collections.Generic;
using System.Text;

namespace Jtfer.Ecp
{
    sealed class ObjectIndexer<TBase>
    {
        static int nextIndex;
        static ObjectIndexer()
        {
            nextIndex = 0;
        }
        internal int Index { get; private set; }
        internal ObjectIndexer()
        {
            Index = nextIndex;
            nextIndex++;
        }
    }

    static class ObjectIndexer<TImplementation, TBase>
        where TImplementation : TBase
    {
        internal static ObjectIndexer<TBase> Indexer { get; private set; }

        static ObjectIndexer()
        {
            Indexer = ObjectIndexManager<TBase>.GetIndexerFor<TImplementation>();
            if (Indexer == null)
            {
                Indexer = new ObjectIndexer<TBase>();
                ObjectIndexManager<TBase>.SetIndexerFor<TImplementation>(Indexer);
            }
        }
    }

    internal static class ObjectIndexManager<TBase>
    {
        public static int TotalTypes { get; private set; }
        static readonly Dictionary<Type, ObjectIndexer<TBase>> _indexers = new Dictionary<Type, ObjectIndexer<TBase>>();

        public static void SetIndexerFor<T>(ObjectIndexer<TBase> indexer)
        {
            _indexers.Add(typeof(T), indexer);
        }

        public static ObjectIndexer<TBase> GetIndexerFor<T>()
            where T : TBase
        {
            return GetIndexerFor(typeof(T));
        }
        public static ObjectIndexer<TBase> GetIndexerFor(Type context)
        {
            if (!_indexers.TryGetValue(context, out ObjectIndexer<TBase> result))
            {
                TotalTypes++;
                result = new ObjectIndexer<TBase>();
                _indexers.Add(context, result);
            }
            return result;
        }
    }
}
