namespace SYSGES_MAGs.Models.ModelsDto
{
    public class TypeMagWithSyntheseDto
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public string Email { get; set; }
        public DateTimeOffset PeriodeDebut { get; set; }
        public DateTimeOffset PeriodeFin { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        // Liste de synthèses par code carte
        public List<BkmvtiSyntheseDto> SyntheseBkmvtis { get; set; } = new List<BkmvtiSyntheseDto>();
    }
}
