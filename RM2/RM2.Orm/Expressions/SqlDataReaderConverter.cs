using MyMiniOrm.Reflections;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;

namespace MyMiniOrm.Expressions
{
    public class SqlDataReaderConverter<T> where T : class, new()
    {
        private static readonly Dictionary<string, Func<SqlDataReader, T>> Dict
            = new Dictionary<string, Func<SqlDataReader, T>>();

        public MyEntity Master { get; set; }

        public List<string> Includes { get; set; }

        public string Key { get; set; }

        public SqlDataReaderConverter(string[] props = null)
        {
            Master = MyEntityContainer.Get(typeof(T));

            if (props == null || props.Length == 0)
            {
                Includes = new List<string>();
                Key = typeof(T).Name;
            }
            else
            {
                Includes = props.ToList();
                Key = typeof(T).Name + "-" + string.Join("-", props.OrderBy(p => p).Distinct());
            }
        }


        #region 反射
        public T ConvertToEntity(SqlDataReader sdr)
        {
            var entity = new T();

            foreach (var property in Master.Properties.Where(p => p.IsMap))
            {
                property.PropertyInfo.SetValue(entity, sdr[property.Name]);
            }

            foreach (var include in Includes)
            {
                var prop = Master.Properties.SingleOrDefault(p => p.Name == include);
                if (prop != null)
                {
                    var subType = prop.PropertyInfo.PropertyType;
                    var subEntityInfo = MyEntityContainer.Get(subType);
                    var subEntity = Activator.CreateInstance(subType);

                    foreach (var subProperty in subEntityInfo.Properties.Where(p => p.IsMap))
                    {
                        subProperty.PropertyInfo.SetValue(subEntity, sdr[$"{include}_{subProperty.Name}"]);
                    }

                    prop.PropertyInfo.SetValue(entity, subEntity);
                }
            }

            return entity;
        }

        public T ConvertToObject(SqlDataReader sdr, List<KeyValuePair<string, string>> keyValuePairs)
        {
            var entity = new T();

            foreach (var property in Master.Properties.Where(p => p.IsMap))
            {
                property.PropertyInfo.SetValue(entity, sdr[property.Name]);
            }

            foreach (var include in Includes)
            {
                var prop = Master.Properties.SingleOrDefault(p => p.Name == include);
                if (prop != null)
                {
                    var subType = prop.PropertyInfo.PropertyType;
                    var subEntityInfo = MyEntityContainer.Get(subType);
                    var subEntity = Activator.CreateInstance(subType);

                    foreach (var subProperty in subEntityInfo.Properties.Where(p => p.IsMap))
                    {
                        subProperty.PropertyInfo.SetValue(subEntity, sdr[$"{include}_{subProperty.Name}"]);
                    }

                    prop.PropertyInfo.SetValue(entity, subEntity);
                }
            }

            return entity;
        }
        #endregion

        #region 表达式目录树
        public Func<SqlDataReader, T> GetFunc(SqlDataReader sdr)
        {
            if (!Dict.TryGetValue(Key, out var func))
            {
                var sdrParameter = Expression.Parameter(typeof(SqlDataReader), "sdr");
                var memberBindings = new List<MemberBinding>();
                var subMemberMaps = new Dictionary<string, List<IncludePropertySdrMap>>();
                foreach (var include in Includes)
                {
                    subMemberMaps.Add(include, new List<IncludePropertySdrMap>());
                }

                for (var i = 0; i < sdr.FieldCount; i++)
                {
                    var fieldName = sdr.GetName(i);
                    var fieldNames = fieldName.Split('_');

                    if (fieldNames.Length == 1)
                    {
                        var property = Master.Properties.SingleOrDefault(p => p.Name == fieldName);
                        if (property != null)
                        {
                            var methodName = GetSdrMethodName(property.PropertyInfo.PropertyType);
                            var methodCall = Expression.Call(sdrParameter,
                                typeof(SqlDataReader).GetMethod(methodName) ?? throw new InvalidOperationException(),
                                Expression.Constant(i));

                            Expression setValueExpression;
                            if (property.PropertyInfo.PropertyType.IsGenericType &&
                                property.PropertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                            {
                                setValueExpression = Expression.Convert(methodCall, property.PropertyInfo.PropertyType);
                            }
                            else
                            {
                                setValueExpression = methodCall;
                            }

                            //memberBindings.Add(Expression.Bind(property.PropertyInfo, methodCall));
                            memberBindings.Add(
                                Expression.Bind(
                                    property.PropertyInfo,
                                    Expression.Condition(
                                        Expression.TypeIs(
                                            Expression.Call(
                                                sdrParameter,
                                                typeof(SqlDataReader).GetMethod("get_Item", new[] {typeof(int)}) ??
                                                throw new InvalidOperationException(),
                                                Expression.Constant(i)),
                                            typeof(DBNull)
                                        ),
                                        Expression.Default(property.PropertyInfo.PropertyType),
                                        setValueExpression
                                    )
                                )
                            );
                        }
                    }
                    else
                    {
                        if (subMemberMaps.TryGetValue(fieldNames[0], out var list))
                        {
                            list.Add(new IncludePropertySdrMap { PropertyName = fieldNames[1], Index = i });
                        }
                    }
                }

                foreach (var include in subMemberMaps)
                {
                    var prop = Master.Properties.SingleOrDefault(p => p.Name == include.Key);
                    if (prop != null)
                    {
                        var subEntityInfo = MyEntityContainer.Get(prop.PropertyInfo.PropertyType);
                        var subBindingList = new List<MemberBinding>();
                        foreach (var subProperty in subEntityInfo.Properties.Where(p => p.IsMap))
                        {
                            var mapper = include.Value.SingleOrDefault(v => v.PropertyName == subProperty.Name);
                            if (mapper != null)
                            {
                                var methodName = GetSdrMethodName(subProperty.PropertyInfo.PropertyType);
                                var methodCall = Expression.Call(
                                    sdrParameter,
                                    typeof(SqlDataReader).GetMethod(methodName) ?? throw new InvalidOperationException(),
                                    Expression.Constant(mapper.Index));

                                Expression setValueExpression;
                                if (subProperty.PropertyInfo.PropertyType.IsGenericType &&
                                    subProperty.PropertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                                {
                                    setValueExpression = Expression.Convert(methodCall, subProperty.PropertyInfo.PropertyType);
                                }
                                else
                                {
                                    setValueExpression = methodCall;
                                }

                                subBindingList.Add(
                                    Expression.Bind(
                                        subProperty.PropertyInfo,
                                        Expression.Condition(
                                            Expression.TypeIs(
                                                Expression.Call(
                                                    sdrParameter,
                                                    typeof(SqlDataReader).GetMethod("get_Item", new[] { typeof(int) }) ?? throw new InvalidOperationException(),
                                                    Expression.Constant(mapper.Index)),
                                                typeof(DBNull)
                                            ),
                                            Expression.Default(subProperty.PropertyInfo.PropertyType),
                                            setValueExpression
                                        )
                                    )
                                );
                            }
                        }

                        var subInitExpression = Expression.MemberInit(Expression.New(prop.PropertyInfo.PropertyType),
                            subBindingList);
                        memberBindings.Add(Expression.Bind(prop.PropertyInfo, subInitExpression));
                    }
                }

                var initExpression = Expression.MemberInit(Expression.New(typeof(T)), memberBindings);
                func = Expression.Lambda<Func<SqlDataReader, T>>(initExpression, sdrParameter).Compile();
                Dict.Add(Key, func);
            }
            else
            {
                Console.WriteLine("应用了缓存");
            }
            return func;
        }

        public T ConvertToEntity2(SqlDataReader sdr)
        {
            if (sdr.HasRows)
            {
                var func = GetFunc(sdr);
                if (sdr.Read())
                {
                    return func.Invoke(sdr);
                }
            }
            return default(T);
        }

        public List<T> ConvertToEntityList(SqlDataReader sdr)
        {
            var result = new List<T>();
            if (!sdr.HasRows)
            {
                return result;
            }

            var func = GetFunc(sdr);
            do
            {
                while (sdr.Read())
                {
                    result.Add(func(sdr));
                }
            } while (sdr.NextResult());
            

            return result;
        }

        public List<T> ConvertToEntityList2(SqlDataReader sdr)
        {
            var result = new List<T>();
            while (sdr.Read())
            {
                result.Add(ConvertToEntity2(sdr));
            }
            return result;
        }

        /// <summary>
        /// 获取SqlDataReader转实体属性时调用的方法名
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private string GetSdrMethodName(Type type)
        {
            var realType = GetRealType(type);
            string methodName;

            if (realType == typeof(string))
            {
                methodName = "GetString";
            }
            else if (realType == typeof(int))
            {
                methodName = "GetInt32";
            }
            else if (realType == typeof(DateTime))
            {
                methodName = "GetDateTime";
            }
            else if (realType == typeof(decimal))
            {
                methodName = "GetDecimal";
            }
            else if (realType == typeof(Guid))
            {
                methodName = "GetGuid";
            }
            else if (realType == typeof(bool))
            {
                methodName = "GetBoolean";
            }
            else
            {
                throw new ArgumentException($"不受支持的类型:{type.FullName}");
            }

            return methodName;
        }

        private static Type GetRealType(Type type)
        {
            var realType = type.IsGenericType &&
                           type.GetGenericTypeDefinition() == typeof(Nullable<>)
                ? type.GetGenericArguments()[0]
                : type;

            return realType;
        }
        #endregion
    }
}
