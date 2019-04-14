using System.Reflection;

namespace MyMiniOrm.Reflections
{
    public class MyProperty
    {
        public string Name { get; set; }

        public PropertyInfo PropertyInfo { get; set; }

        public string TypeName { get; set; }

        public string FieldName { get; set; }

        public bool IsKey { get; set; }

        public bool IsMap { get; set; } = true;

        public bool InsertIgnore { get; set; }

        public bool UpdateIgnore { get; set; }

        public bool JoinAble { get; set; }

        public string ForeignKey { get; set; }

        public string MasterKey { get; set; }

        public MyProperty(PropertyInfo property)
        {
            Name = property.Name;
            TypeName = property.PropertyType.Name;
            PropertyInfo = property;

            if (property.IsMapAble())
            {
                // 判断是否主键
                var keyAttribute = property.GetKeyAttribute();
                if (keyAttribute != null)
                {
                    // 有
                    IsKey = true;
                    FieldName = string.IsNullOrWhiteSpace(keyAttribute.FieldName) ? Name : keyAttribute.FieldName;
                    if (keyAttribute.IsIncrement)
                    {
                        // 如果是自增列，不能插入和修改
                        InsertIgnore = true;
                        UpdateIgnore = true;
                    }
                    else
                    {
                        // 如果不是自增列，可插入但不能修改
                        InsertIgnore = true;
                    }
                }
                else if (Name == "Id")
                {
                    FieldName = "Id";
                    IsKey = true;
                    InsertIgnore = true;
                    UpdateIgnore = true;
                }
                else
                {
                    // 可映射的属性
                    var columnAttribute = property.GetMyColumnAttribute();

                    if (columnAttribute != null)
                    {
                        FieldName = string.IsNullOrWhiteSpace(columnAttribute.ColumnName)
                            ? Name
                            : columnAttribute.ColumnName;
                        InsertIgnore = columnAttribute.Ignore || columnAttribute.InsertIgnore;
                        UpdateIgnore = columnAttribute.Ignore || columnAttribute.UpdateIgnore;
                    }
                    else
                    {
                        FieldName = Name;
                    }
                }
            }
            else if (property.IsJoinAble())
            {
                // 可关联查询的属性
                IsMap = false;
                JoinAble = true;
                UpdateIgnore = true;
                InsertIgnore = true;
                var foreignAttribute = property.GetForeignKeyAttribute();
                if (foreignAttribute == null)
                {
                    ForeignKey = Name + "Id";
                    MasterKey = "Id";
                }
                else
                {
                    ForeignKey = string.IsNullOrWhiteSpace(foreignAttribute.ForeignKey)
                        ? Name + "Id"
                        : foreignAttribute.ForeignKey;
                    MasterKey = string.IsNullOrWhiteSpace(foreignAttribute.MasterKey)
                        ? "Id"
                        : foreignAttribute.MasterKey;
                }
            }
            else
            {
                // 其他属性
                IsMap = false;
                UpdateIgnore = true;
                InsertIgnore = true;
            }
        }
    }
}
