using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using MyMiniOrm.Reflections;

namespace MyMiniOrm.Expressions
{
    public class ObjectExpressionVisitor : ExpressionVisitor
    {
        private readonly List<KeyValuePair<string, string>> _propertyList;

        private readonly MyEntity _master;

        public ObjectExpressionVisitor(MyEntity master)
        {
            _propertyList = new List<KeyValuePair<string, string>>();
            _master = master;
        }

        public List<KeyValuePair<string, string>> GetPropertyList()
        {
            return _propertyList;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            var property = _master.Properties.SingleOrDefault(p => p.Name == node.Member.Name);
            if (property != null)
            {
                _propertyList.Add(new KeyValuePair<string, string>(property.Name, property.FieldName));
            }

            return node;
        }

        protected override Expression VisitNew(NewExpression node)
        {
            var members = node.Members.Select(m => m.Name);
            var props = _master.Properties.Where(p => p.IsMap && members.Contains(p.Name)).Select(p => new KeyValuePair<string, string>(p.Name, p.FieldName));
            _propertyList.AddRange(props);
            return node;
        }
    }
}
