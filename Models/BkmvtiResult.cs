namespace SYSGES_MAGs.Models
{
    public class BkmvtiResult
    {
        public string NumeroCompte { get; set; } = string.Empty;
        public string CodeAgence { get; set; } = string.Empty;
        public string LibelleCarte { get; set; } // libelle/nom de la carte Ex : Ext. Horizon jan. 2025/ Redev Horizon avr. 2025/...
        public DateTimeOffset DatePrelevement { get; set; }
        public long Total { get; set; }
    }
}
