using System;
using System.Collections.Concurrent;

namespace MyMiniOrm.Reflections
{
    public class MyEntityContainer
    {
        private static readonly ConcurrentDictionary<string, MyEntity> Dict = 
            new ConcurrentDictionary<string, MyEntity>();

        public static MyEntity Get(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (Dict.TryGetValue(type.FullName ?? throw new InvalidOperationException(), out var result))
            {
                return result;
            }
            else
            {
                var entity = new MyEntity(type);
                Dict.TryAdd(type.FullName, entity);
                return entity;
            }
        }
    }
}
