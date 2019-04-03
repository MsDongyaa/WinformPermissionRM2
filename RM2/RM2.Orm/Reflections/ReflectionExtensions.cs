using MyMiniOrm.Attributes;
using System.Reflection;

namespace MyMiniOrm.Reflections
{
    public static class ReflectionExtensions
    {
        public static MyKeyAttribute GetKeyAttribute(this PropertyInfo property)
        {
            return property.GetCustomAttribute<MyKeyAttribute>();
        }

        public static MyForeignKeyAttribute GetForeignKeyAttribute(this PropertyInfo property)
        {
            return property.GetCustomAttribute<MyForeignKeyAttribute>();
        }

        public static MyColumnAttribute GetMyColumnAttribute(this PropertyInfo property)
        {
            return property.GetCustomAttribute<MyColumnAttribute>();
        }

        public static bool IsMapAble(this PropertyInfo property)
        {
            return property.PropertyType.IsValueType || property.PropertyType == typeof(string);
        }

        public static bool IsJoinAble(this PropertyInfo property)
        {
            return property.PropertyType.IsClass && property.PropertyType != typeof(string) && !property.PropertyType.IsGenericType;
        }
    }
}
