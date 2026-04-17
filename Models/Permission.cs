//namespace SYSGES_MAGs.Models
//{
//    public class Permission
//    {
//        public int Id { get; set; }

//        // Code unique utilisé dans le code (ex: CREATE_USER)
//        public string Code { get; set; }

//        // Nom lisible
//        public string Name { get; set; }

//        // Description fonctionnelle
//        public string Description { get; set; }

//        // Pour regrouper (ex: Users, Orders, Products)
//        public string Module { get; set; }

//        // Audit
//        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
//        public DateTimeOffset CreatedBy { get; set; } = DateTimeOffset.UtcNow;

//        public DateTimeOffset? UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
//        // Navigation
//        public ICollection<RolePermission> RolePermissions { get; set; }

//    }
//}
