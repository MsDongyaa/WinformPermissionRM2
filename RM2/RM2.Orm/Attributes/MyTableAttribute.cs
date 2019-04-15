using System;

namespace MyMiniOrm.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MyTableAttribute : Attribute
    {
        public string TableName { get; }

        public MyTableAttribute(string tableName)
        {
            TableName = tableName;
        }
    }
}
