using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using MyMiniOrm.Commons;
using MyMiniOrm.Expressions;
using MyMiniOrm.Reflections;

namespace MyMiniOrm.Updateable
{
    public class MyUpdateable<T> where T : IEntity
    {
        private readonly MyEntity _entity;

        private readonly List<T> _entityList;

        private readonly List<string> _ignorePropertyList;

        private readonly List<string> _includePropertyList;

        /// <summary>
        /// 指定列调用类型，0未指定 1包含 2忽略
        /// </summary>
        private int _includeOrIgnore;

        private bool _ignoreAttribute;

        public MyUpdateable()
        {
            _entity = MyEntityContainer.Get(typeof(T));
            _entityList = new List<T>();
            _ignorePropertyList = new List<string>();
            _includePropertyList = new List<string>();
            _includeOrIgnore = 0;
            _ignoreAttribute = true;
        }

        public MyUpdateable(T entity) : this()
        {
            _entityList.Add(entity);
        }

        public MyUpdateable(IEnumerable<T> entityList) : this()
        {
            _entityList = entityList.ToList();
        }

        public MyUpdateable<T> Include(Expression<Func<T, object>> expression, bool ignoreAttribute = true)
        {
            if (_includeOrIgnore != 0)
            {
                throw new Exception("Include和Ignore方法只能调用一次");
            }

            var visitor = new ObjectExpressionVisitor(_entity);
            visitor.Visit(expression);
            _includePropertyList.AddRange(visitor.GetPropertyList().Select(kv => kv.Key));
            _includeOrIgnore = 1;
            _ignoreAttribute = ignoreAttribute;
            return this;
        }

        public MyUpdateable<T> Ignore(Expression<Func<T, object>> expression, bool ignoreAttribute = true)
        {
            if (_includeOrIgnore != 0)
            {
                throw new Exception("Include和Ignore方法只能调用一次");
            }

            var visitor = new ObjectExpressionVisitor(_entity);
            visitor.Visit(expression);
            _includePropertyList.AddRange(visitor.GetPropertyList().Select(kv => kv.Key));
            _includeOrIgnore = 2;
            _ignoreAttribute = ignoreAttribute;
            return this;
        }

        public int Save()
        {
            if (_entityList.Count == 0)
            {
                return 0;
            }
            else if(_entityList.Count == 1)
            {
                var entity = _entityList.First();

            }

            return 0;
        }

        private string GetUpdateSql()
        {
            var props = _entity.Properties;
            var sb = new StringBuilder($"UPDATE [{_entity.TableName}] SET ");
            if (_includeOrIgnore == 0)
            {
                sb.Append(string.Join(",",
                    props.Where(p => !p.UpdateIgnore).Select(p => $"[{p.FieldName}]=@{p.Name}")));
            }

            sb.Append(" WHERE Id=@Id");
            return sb.ToString();
        }
    }
}
