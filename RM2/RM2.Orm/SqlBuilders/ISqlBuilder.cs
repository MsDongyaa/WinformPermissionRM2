using MyMiniOrm.Commons;
using MyMiniOrm.Reflections;

namespace MyMiniOrm.SqlBuilders
{
    public interface ISqlBuilder
    {
        string Select(string table, string columns, string where, string sort, int top = 0);

        string PagingSelect(string table, string columns, string where, string sort, int pageIndex, int pageSize);

        string Insert(MyEntity entityInfo);

        string Update(MyEntity entityInfo);

        string UpdateIgnore(MyEntity entityInfo, string[] propertyList, bool ignoreAttribute, string where);

        string UpdateInclude(MyEntity entityInfo, string[] propertyList, bool ignoreAttribute, string where);

        string Update(string table, DbKvs kvs, string where);

        string Delete(string table, string where);

        string Delete(MyEntity entityInfo, string where);

        string GetCount(string table, string where);

        string GetCount(MyEntity entityInfo, string where);
    }
}
