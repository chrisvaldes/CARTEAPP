namespace SYSGES_MAGs.Models.ModelsDto
{
    public class BkPrdCliDto
    {
        public string? cpro { get; set; } 
        public string? ncp { get; set; } // numéro de compte rattagé
        public DateTime? ddsou { get; set; } // date de début de souscription
        public DateTime? dfsou { get; set; } // date de fin de souscription
        public string? ctar { get; set; } // code de tarification
        public string? ttar { get; set; } // type de tarification
        public string? ncpf { get; set; } // Numéro de compte devant supporter la facturation 
        public string? eta { get; set; } // etat de la souscription 

    }
}
