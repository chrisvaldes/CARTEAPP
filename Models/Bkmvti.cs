namespace SYSGES_MAGs.Models
{
    public class Bkmvti
    {
        public Guid Id { get; set; }
        public Guid TypeMag { get; set; }
        public string CodeAgence { get; set; } // 04000
        public string Sequence { get; set; } // 001
        public string? CodeIN { get; set; } // IN3
        public string? CodeDevise { get; set; } // 950
        public string? EstActif { get; set; } // N
        public string NumeroCompte { get; set;} // **********
        public string TypeBeneficiaire { get; set; } // AUTO
        public int ReferenceBeneficiaire { get; set; } // 691228
        public int CleBeneficiaire { get; set; } // 46/10/97...
        public DateTimeOffset DatePrelevement { get; set; } // date de recupération des MAGs
        public long PrixUnitCarte { get; set; } // 5000 => prix mensuel de la carte associée ou pas à un pack
        public string ReferenceOperation { get; set; } // RVSA250610
        public string CodeOperation { get; set; } // C => code opération
        public string CodeEmetteur { get; set; } // FACSER => code émetteur de la carte
        public string IndicateurDomiciliation { get; set; } // N
        public string LibelleCarte { get; set; } // libelle/nom de la carte Ex : Ext. Horizon jan. 2025/ Redev Horizon avr. 2025/...
        public string DesignationCarte { get; set; } // visa premier, horizon...
        public string Carte { get; set; } // numéro de la carte
        public DateTimeOffset DateValiditeCarte { get; set; }
        public DateTimeOffset DateCreationCarte {get;set;}
        public string CodeTarif {get;set;}
        public string CodeCarte {get;set;}
        public DateTimeOffset? startPeriod  { get; set; }
        public DateTimeOffset? endPeriod  { get; set; }

    }
}
