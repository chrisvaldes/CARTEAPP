//namespace SYSGES_MAGs.Models
//{
//    public class Role
//    {
//        public int Id { get; set; }

//        public string Name { get; set; }        // ex: Admin
//        public string Code { get; set; }        // ex: ADMIN
//        public string Description { get; set; } 
//        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
//        public DateTimeOffset CreatedBy { get; set; } = DateTimeOffset.UtcNow;

//        public DateTimeOffset? UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
//        public string UpdatedBy { get; set; }

//        // Navigation
//        public ICollection<RolePermission> RolePermissions { get; set; }
//    }
//}
