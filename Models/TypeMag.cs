namespace SYSGES_MAGs.Models
{
    public class TypeMag
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public string Email { get; set; }
        public bool isAlreadyDownload { get; set; } = false;
        public DateTimeOffset PeriodeDebut { get; set; }
        public DateTimeOffset PeriodeFin { get; set; } 
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
