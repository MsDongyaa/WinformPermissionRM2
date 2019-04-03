namespace MyMiniOrm.Commons
{
    public class FullEntity : AuditEntity, ISoftDelete
    {
        public bool IsDel { get; set; }
    }
}
