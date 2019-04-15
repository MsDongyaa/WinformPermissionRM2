using System;

namespace MyMiniOrm.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class MyForeignKeyAttribute : Attribute
    {
        public string ForeignKey { get; set; }

        public string MasterKey { get; set; } = "Id";

        public MyForeignKeyAttribute(string foreignKey)
        {
            ForeignKey = foreignKey;
        }
    }
}
