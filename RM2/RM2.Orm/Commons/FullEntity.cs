namespace RM2.Orm.Commons
{
    public class FullEntity : AuditEntity, ISoftDelete
    {
        public bool IsDel { get; set; }
    }
}
