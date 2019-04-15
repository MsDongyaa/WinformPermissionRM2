using MyMiniOrm.Attributes;
using MyMiniOrm.Commons;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyMiniOrm.Reflections
{
    public class MyEntity
    {
        public string KeyColumn { get; set; }

        public string Name { get; set; }

        public string TableName { get; set; }

        public bool IsSoftDelete { get; set; }

        public bool IsCreateAudit { get; set; }

        public bool IsUpdateAudit { get; set; }

        public List<MyProperty> Properties { get; set; }

        public MyEntity(Type type)
        {
            Name = type.Name;
            IsSoftDelete = type.IsInstanceOfType(typeof(ISoftDelete));
            IsCreateAudit = type.IsInstanceOfType(typeof(ICreateAudit));
            IsUpdateAudit = type.IsInstanceOfType(typeof(IUpdateAudit));

            var tableAttr = type.GetCustomAttributes(typeof(MyTableAttribute), false);
            if (tableAttr.Length > 0)
            {
                var tableName = ((MyTableAttribute)tableAttr[0]).TableName;
                TableName = string.IsNullOrWhiteSpace(tableName) ? type.Name.Replace("Entity", "") : tableName;
            }
            else
            {
                TableName = Name;
            }

            Properties = type.GetProperties().Select(p => new MyProperty(p)).ToList();
            var keyProperty = Properties.SingleOrDefault(p => p.IsKey);
            if (keyProperty == null) throw new ArgumentNullException(nameof(keyProperty), "实体必须有一个主键列");
            KeyColumn = keyProperty.FieldName;
        }
    }
}
