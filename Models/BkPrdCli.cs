namespace SYSGES_MAGs.Models
{
    public class BkPrdCli
    {
        public string? cli { get; set; }
        public string? cpro { get; set; }
        public string? cpack { get; set; }
        public string? rsou { get; set; }
        public string? modu { get; set; }
        public string? tdos { get; set; }
        public string? ndos { get; set; }
        public string? age { get; set; }
        public string? dev { get; set; }
        public string? suf { get; set; }
        public string? utsou { get; set; }
        public string? jour { get; set; }
        public string? agef{ get; set; }
        public string? devf { get; set; }
        public string? suff { get; set; }
        public string? dpe { get; set; }
        public string? nbe { get; set; }
        public string? dde { get; set; }
        public string? dex { get; set; }
        public string? qte { get; set; }
        public string? fmep { get; set; }
        public string? eve { get; set; }
        public string? obs { get; set; }
        public string? ctax { get; set; }
        public string? nanti { get; set; }
        public string? menc { get; set; }
        public string? dme { get; set; }
        public string? uti { get; set; }
        public DateTime? dou { get; set; }
        public DateTime? dmo { get; set; }
        public string? atrf { get; set; }
        public string? mhtmep { get; set; }
        public string? recfac { get; set; }
        public string? dctar { get; set; }
        public string? devmep { get; set; }
        public string? rsou_pack { get; set; }
        public string? ncp { get; set; } // numéro de compte rattagé
        public DateTime? ddsou { get; set; } // date de début de souscription
        public DateTime? dfsou { get; set; } // date de fin de souscription
        public string? ctar { get; set; } // code de tarification
        public string? ttar { get; set; } // type de tarification
        public string? ncpf { get; set; } // Numéro de compte devant supporter la facturation
        public string? cnet { get; set; } // cumul des montants débités
        public string? eta { get; set; } // etat de la souscription
        public string? ctr { get; set; } // code de traitement
        public string? cqtef { get; set; } // cumul des quantités facturées
    }
}
