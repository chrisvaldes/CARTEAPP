using System.Globalization;
using System.Text;
using OfficeOpenXml;
using SYSGES_MAGs.Data;
using SYSGES_MAGs.Models;
using SYSGES_MAGs.Models.ModelsDto;
using SYSGES_MAGs.Repository;
using SYSGES_MAGs.Repository.IRepository;
using SYSGES_MAGs.Services.IServices;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SYSGES_MAGs.Services
{
    public class MagProcessingService : IMagProcessingService
    {
        private readonly ILogger<MagProcessingService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IBkPrdCliRepository _bkPrdCliRepository;
        private readonly ITypeMagRepository _typeMagRepository;
        private readonly IBkmvtiRepository _bkmvtiRepository;
        private readonly ApplicationDbContext _dbContext;
        private readonly IEmailService _emailService;
        public long prixMensuelCarte = 0;

        // Injection via constructeur
        public MagProcessingService(
            ILogger<MagProcessingService> logger,
            IHttpContextAccessor httpContextAccessor,
            IBkPrdCliRepository bkPrdCliRepository,
            ITypeMagRepository typeMagRepository,
            IBkmvtiRepository kmvtiRepository,
            IServiceScopeFactory serviceScopeFactory,
            ApplicationDbContext context,
            IEmailService emailService
        )
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _bkPrdCliRepository = bkPrdCliRepository;
            _typeMagRepository = typeMagRepository;
            _bkmvtiRepository = kmvtiRepository;
            _emailService = emailService;
            _dbContext = context;
        }

        Dictionary<string, ComptesActifsResponse> comptesActifs =
            new Dictionary<string, ComptesActifsResponse>();
        Dictionary<string, ComptesOuvertsResponse> comptesOuverts =
            new Dictionary<string, ComptesOuvertsResponse>();
        Dictionary<string, DateDsouPackEchuResponse> dateDersouPackEchus =
            new Dictionary<string, DateDsouPackEchuResponse>();
        Dictionary<string, HistCptDebiteRedevCarteResponse> histCptDebiteRedevCartes =
            new Dictionary<string, HistCptDebiteRedevCarteResponse>();
        Dictionary<string, PackagesActifsResponse> packActifs =
            new Dictionary<string, PackagesActifsResponse>();
        List<BkPrdCliDto> cliCartePackage;
        string numeroCompte = "";

        //// EXTRACTION DE LA DATE DE CREATION DE LA CARTE
        DateTimeOffset? dateCreationCarte;

        // Création de la date avec le 1er jour du mois
        DateTimeOffset? dateValiditeCarte;

        Dictionary<string, List<Apprints>> clientPlusUneCarte =
            new Dictionary<string, List<Apprints>>();

        Dictionary<string, List<Bkmvti>> fichierComptatble = new Dictionary<string, List<Bkmvti>>();

        Dictionary<string, string> codeTarifNom = new Dictionary<string, string>()
        {
            { "CL012", "CARTE HORIZON" },
            { "CL011", "VISA PLATINUM" },
            { "CL014", "VISA FREE" },
            { "CL015", "VISA YOUNG" },
            { "CL006", "VISA PREMIER" },
            { "CL007", "VISA CLASSIC" },
            { "PR011", "VISA PLATINUM" },
            { "PR012", "CARTE HORIZON" },
            { "PR013", "VISA HORIZON PREPAYEE" },
            { "PR016", "VISA INFINITE" },
            { "PR005", "VISA BUSINESS" },
            { "PR006", "VISA PREMIER" },
            { "PR007", "VISA CLASSIC" },
            { "C3011", "CARTE PLATINUM" },
            { "C3012", "CARTE HORIZON" },
            { "C3016", "CARTE INFINITE" },
            { "C3006", "VISA PREMIER" },
            { "C3007", "VISA CLASSIC" },
            { "EX011", "CARTE PLATINUM" },
            { "EX012", "CARTE HORIZON" },
            { "EX013", "VISA HORIZON PREPAYEE" },
            { "EX016", "CARTE INFINITE" },
            { "EX005", "VISA BUSINNESS" },
            { "EX006", "VISA PREMIER" },
            { "EX007", "VISA CLASSIC" },
        };

        Dictionary<string, long> cartePrix = new Dictionary<string, long>
        {
            { "012", 2504 },
            { "007", 4770 },
            { "006", 8705 },
            { "011", 14906 },
            { "016", 29813 },
        };

        Dictionary<string, string> cartePackage = new Dictionary<string, string>
        {
            { "012", "300001" }, // Carte visa horizon
            { "007", "300007" }, // Carte visa classic
            { "006", "300004" }, // Carte visa premier
            { "011", "300007" }, // Carte visa platinum
            { "016", "300008" }, // Carte visa infinite
        };

        // exlusion des cartes visa free, young, businnes
        string[] cartesExclu = new[] { "014", "015", "005" };

        public async Task<ServiceResult<string>> ProcessTxtExcelFiles(
            IFormFile apprintFile,
            IFormFile openAccountFile,
            IFormFile activeAccount,
            IFormFile dateLastSouPackEchuFile,
            IFormFile activePackageFile,
            IFormFile accountHisDebiteByRedevCardFile,
            string typeMag,
            DateTime startPeriod,
            DateTime endPeriod
        )
        {
            // Variable "apprints" qui doit contenir chaque ligne du fichier apprint
            List<Apprints> apprints = new List<Apprints>();
            // Variable qui va contenir la liste des bkmvtis
            List<Bkmvti> bkmvtis = new List<Bkmvti>();
            DateTime dateDebutExecution = DateTime.Now;
            // Variable qui va contenir la période de début d'étude du mag
            DateTime start;
            // Variable qui va contenir la période de fin d'étude mag
            DateTime end;
            // Variable qui va compter le nombre de ligne de l'apprint.
            long ligne = 0;

            try
            {
                // La période d'étude doit être au minimum 1 mois, et la période de fin d'étude doit être > au début d'étude
                var dureePeriode = NombreMois(startPeriod, endPeriod);

                if ((startPeriod > endPeriod) || (dureePeriode < 1))
                {
                    return new ServiceResult<string>
                    {
                        Success = false,
                        Message =
                            "La période de début doit être inférieur à la période de fin, et la différence >= 1",
                    };
                }

                // Vérifier si le manque à gagner n'a pas encore été récupérer sur la période.
                TypeMag? existTypeMag = await _typeMagRepository.IsTypeMagExist(startPeriod);

                // si oui, retourné un message informantif
                if (existTypeMag != null)
                {
                    return new ServiceResult<string>
                    {
                        Success = false,
                        Message = "Manque à gagner déjà récupérer sur la période.",
                    };
                }

                // si le mag n'a pas encore été récupérer, continuer le traitement.

                // lecture du fichier apprint
                using var reader = new StreamReader(apprintFile.OpenReadStream());

                //lecture des différents fichiers excel contenant les requetes des bases de données
                using var compteOuvert = new ExcelPackage(openAccountFile.OpenReadStream());
                using var compteActif = new ExcelPackage(activeAccount.OpenReadStream());
                using var dateDerniereSouPackEchu = new ExcelPackage(
                    dateLastSouPackEchuFile.OpenReadStream()
                );
                using var packageActif = new ExcelPackage(activePackageFile.OpenReadStream());
                using var histCptDebiteParRedevCarte = new ExcelPackage(
                    accountHisDebiteByRedevCardFile.OpenReadStream()
                );

                //lecture de la première feuille de calcule excel
                var worksheetCompteActif = compteActif.Workbook.Worksheets[0];
                comptesActifs = GetComptesActifs(worksheetCompteActif);

                var worksheetCompteOuvert = compteOuvert.Workbook.Worksheets[0];
                comptesOuverts = GetComptesOuvert(worksheetCompteOuvert);

                var worksheetDsouPackEchu = dateDerniereSouPackEchu.Workbook.Worksheets[0];
                dateDersouPackEchus = GetDsouPackEchu(worksheetDsouPackEchu);

                var worksheetHistCptDebiteRedev = histCptDebiteParRedevCarte.Workbook.Worksheets[0];
                histCptDebiteRedevCartes = GetHistCptDebiteRedevCarte(worksheetHistCptDebiteRedev);

                var worksheetPackActif = packageActif.Workbook.Worksheets[0];
                packActifs = GetPackagesActifs(worksheetPackActif);

                // Conversion des périodes d'étude
                if (
                    DateTime.TryParse(startPeriod.ToString(), out DateTime startPer)
                    && DateTime.TryParse(endPeriod.ToString(), out DateTime endPer)
                )
                {
                    start = startPer;
                    end = endPer;

                    using var transaction = await _dbContext.Database.BeginTransactionAsync();

                    try
                    {
                        //sauvegarde du type de manque à gagner effectué
                        TypeMag typeMagResult = await _typeMagRepository.SaveTypeMagAsync(
                            new TypeMag
                            {
                                Description =
                                    "cap_"
                                    + start.Day.ToString("D2")
                                    + "_"
                                    + start.ToString("MMM", new CultureInfo("fr-FR"))
                                    + "_"
                                    + start.Year
                                    + "_"
                                    + end.Day.ToString("D2")
                                    + "_"
                                    + end.ToString("MMM", new CultureInfo("fr-FR"))
                                    + "_"
                                    + end.Year,
                                Email =
                                    _httpContextAccessor.HttpContext?.User?.Identity?.Name
                                    ?? "unknown",
                                PeriodeDebut = DateTime.SpecifyKind(startPeriod, DateTimeKind.Utc),
                                PeriodeFin = DateTime.SpecifyKind(endPeriod, DateTimeKind.Utc),
                            }
                        );
                        _logger.LogInformation("type mag :  " + typeMagResult.Id);

                        // ligne d'en-tête
                        string header = await reader.ReadLineAsync() ?? string.Empty;

                        // ligne 1 => en-tête du fichier apprint
                        ligne++;
                        _logger.LogInformation("ligne :  " + ligne);

                        while (!reader.EndOfStream)
                        {
                            _logger.LogInformation("dans le while : ");
                            // on commence à lire le fichier à la ligne 2
                            ligne++;

                            try
                            {
                                // lecture d'une ligne du fichier apprint
                                var ligneApprint = await reader.ReadLineAsync();

                                // si la ligne est vide, sauté et continué
                                if (string.IsNullOrEmpty(ligneApprint))
                                {
                                    continue;
                                }

                                //Ligne du fichier apprint convertie en un enregistrement Apprints (colones)
                                Apprints apprint = ConvertTxtToApprint(ligneApprint, ligne);

                                // EXTRACTION DU NUMERO DE COMPTE ET DES DATE DE CREATION ET DE VALIDATION DE LA CARTE

                                // extraction du numéro de compte
                                numeroCompte =
                                    apprint.DateValiditeAgenceCodeDeviseNumeroCompte!.Substring(12);

                                //// EXTRACTION DE LA DATE DE CREATION DE LA CARTE
                                dateCreationCarte = GetDateCreationCarte(apprint);

                                // Création de la date avec le 1er jour du mois
                                dateValiditeCarte = getDateValiditeCarte(apprint);

                                // Comparaison avec les dates fournies et autres exclusions
                                if ((dateValiditeCarte <= DateTime.UtcNow) || // carte expirées
                                    (!comptesOuverts.ContainsKey(numeroCompte)) || // comptes fermés
                                    (!packActifs.ContainsKey(numeroCompte)) || // comptes sans package actif
                                    (apprint.CodeCarte != null && cartesExclu.Contains(apprint.CodeCarte)) || // exclusion des cartes visa free, young, business
                                    ((!numeroCompte.StartsWith("02")) && (!numeroCompte.StartsWith("31"))) // exclusion des carte différents de 02 et 31
                                )
                                {
                                    // prévoir ecrire dans un fichier pour les logs. la ligne qui declence une erreur.
                                    _logger.LogInformation($"Ligne {ligne} : Carte invalide");
                                    continue;
                                }
                                else
                                {
                                    // si la carte du client est active, vérifié s'il a plus d'une carte.
                                    if (!clientPlusUneCarte.ContainsKey(numeroCompte))
                                    {
                                        // si le numéro de compte n'exite pas encore, créer une liste pour collecter toutes les occurences
                                        // de carte en fonction de leurs ncpf ou ncp.
                                        clientPlusUneCarte[numeroCompte] = new List<Apprints>();
                                        // ajout de la première carte dans la liste pour ne pas la perdre.
                                        clientPlusUneCarte[numeroCompte]
                                            .Add(
                                                new Apprints
                                                {
                                                    CodeCarte = apprint.CodeCarte,
                                                    NumCarte = apprint.NumCarte,
                                                    NomPropCarte = apprint.NomPropCarte,
                                                    LongNum = apprint.LongNum,
                                                    VhCodeCarte = apprint.VhCodeCarte,
                                                    QZero = apprint.QZero,
                                                    DateCreationCarte = apprint.DateCreationCarte,
                                                    EstActifCodeTarifNumeroCompte =
                                                        apprint.EstActifCodeTarifNumeroCompte,
                                                    NomPrenom = apprint.NomPrenom,
                                                    LastProp = apprint.LastProp,
                                                    DateValiditeAgenceCodeDeviseNumeroCompte =
                                                        apprint.DateValiditeAgenceCodeDeviseNumeroCompte,
                                                }
                                            );
                                    }
                                    else
                                    {
                                        // si le numéro de compte existe déjà, ajouter la carte à la liste correspondante au ncpf
                                        clientPlusUneCarte[numeroCompte]
                                            .Add(
                                                new Apprints
                                                {
                                                    CodeCarte = apprint.CodeCarte,
                                                    NumCarte = apprint.NumCarte,
                                                    NomPropCarte = apprint.NomPropCarte,
                                                    LongNum = apprint.LongNum,
                                                    VhCodeCarte = apprint.VhCodeCarte,
                                                    QZero = apprint.QZero,
                                                    DateCreationCarte = apprint.DateCreationCarte,
                                                    EstActifCodeTarifNumeroCompte =
                                                        apprint.EstActifCodeTarifNumeroCompte,
                                                    NomPrenom = apprint.NomPrenom,
                                                    LastProp = apprint.LastProp,
                                                    DateValiditeAgenceCodeDeviseNumeroCompte =
                                                        apprint.DateValiditeAgenceCodeDeviseNumeroCompte,
                                                }
                                            );
                                    }

                                    //}
                                }
                            }
                            catch (Exception ex)
                            {
                                return new ServiceResult<string>
                                {
                                    Success = false,
                                    Message = $"Erreur à la ligne {ligne} : {ex.Message}",
                                };
                            }
                        }
                        
                        // Une fois l'ensemble du fichier apprint parcouru, parcourir les clients ayant plus d'une carte.
                        foreach (var client in clientPlusUneCarte)
                        {
                            // clé ncpf de la liste des carte du client
                            var ncpf = client.Key;
                            // liste des cartes du client associé au ncpf
                            var cartesClient = client.Value;
                            // si le client n'a qu'une seul carte, ça ne nous interesse pas ou plutard.
                            if (cartesClient.Count <= 1)
                            {
                                continue;
                            }

                            // constituer le couple code package exple "011 => 300001", "016 => 100003"... entre les packages actifs et le ncpf
                            // afin de vérifier si au moins un code de carte match avec un package.
                            var result = BuildCartePackageList(cartesClient, packActifs, ncpf);
                             
                            // si le client à plus d'une carte gratuite avec package alors "if" sinon "else"
                            if (result != null)
                            {

                                // le client à au moins une carte aligner au package? 
                                bool hasAtLeastOneCoherenceCartePackage = result.Any(x =>
                                    cartePackage[x.CodeCarte!] == x.CodePackage
                                );

                                // vérifie si le client à au moins une carte non aligner au package
                                // bool hasNotAtLeastOneCoherence = result.Any(x =>
                                //    cartePackage[x.CodeCarte!] != x.CodePackage
                                //);

                                // si non alors "if" sinon alors "else"
                                if (!hasAtLeastOneCoherenceCartePackage)
                                {
                                    // on parcours la liste des cartes du client
                                    foreach (var carte in cartesClient)
                                    {
                                        // période d'étude
                                        var periodeEtude = new Periode(startPeriod, endPeriod);

                                        var p2 = new Periode(
                                            GetDateCreationCarte(carte)!.Value,
                                            endPeriod
                                        );

                                        // le minimum selectionné entre la période en cours et la carte valide
                                        var minPeriodSelected = MinPeriode(periodeEtude, p2);

                                        int duree = Math.Max(
                                            1,
                                            minPeriodSelected != null
                                                ? NombreMois(
                                                    minPeriodSelected.Debut,
                                                    minPeriodSelected.Fin
                                                ) ?? 0
                                                : 0
                                        );

                                        long total = duree * cartePrix[carte.CodeCarte!];

                                        string codeTarifComplet = BuildCodeTarifComplet(
                                            carte.EstActifCodeTarifNumeroCompte,
                                            carte.CodeCarte
                                        );

                                        string DesignationCarte = DesignationCartes(
                                            codeTarifComplet
                                        );

                                        prixMensuelCarte = PrixUnitaireCarte(carte.CodeCarte!);

                                        bkmvtis.Add(
                                            new Bkmvti
                                            {
                                                NumeroCompte = ncpf,
                                                DateCreationCarte = DateTime.SpecifyKind(
                                                    dateCreationCarte!.Value.LocalDateTime,
                                                    DateTimeKind.Utc
                                                ),
                                                DateValiditeCarte = DateTime.SpecifyKind(
                                                    dateValiditeCarte!.Value.LocalDateTime,
                                                    DateTimeKind.Utc
                                                ),
                                                CodeTarif = codeTarifComplet,
                                                CodeCarte = carte.CodeCarte!,
                                                DesignationCarte = DesignationCarte,
                                                startPeriod = DateTime.SpecifyKind(
                                                    startPeriod,
                                                    DateTimeKind.Utc
                                                ),
                                                endPeriod = DateTime.SpecifyKind(
                                                    startPeriod,
                                                    DateTimeKind.Utc
                                                ), // bkPrdCliDto!.ddsou
                                                TypeMag = typeMagResult.Id,
                                                CodeIN = "IN3",
                                                CodeDevise =
                                                    carte.DateValiditeAgenceCodeDeviseNumeroCompte!.Substring(
                                                        9,
                                                        3
                                                    ),
                                                EstActif =
                                                    carte.EstActifCodeTarifNumeroCompte!.Substring(
                                                        0,
                                                        1
                                                    ),
                                                CodeAgence =
                                                    carte.DateValiditeAgenceCodeDeviseNumeroCompte.Substring(
                                                        4,
                                                        5
                                                    ),
                                                TypeBeneficiaire = "AUTO",
                                                ReferenceBeneficiaire = 691228,
                                                CleBeneficiaire = 46,
                                                DatePrelevement = DateTime.SpecifyKind(
                                                    startPeriod,
                                                    DateTimeKind.Utc
                                                ),
                                                PrixUnitCarte = duree * prixMensuelCarte, // prix mensuel de la carte associée ou pas à un pack
                                                ReferenceOperation =
                                                    "RVSA"
                                                    + start.ToString("yy")
                                                    + start.Month.ToString("D2")
                                                    + start.Day.ToString("D2"),
                                                CodeOperation = "C",
                                                CodeEmetteur = "FACSER",
                                                IndicateurDomiciliation = "N",
                                                LibelleCarte = BuildLibelleCarte(
                                                    carte.EstActifCodeTarifNumeroCompte,
                                                    carte.CodeCarte,
                                                    startPeriod
                                                ),
                                                Carte = carte.NumCarte!,
                                                Sequence = "001",
                                            }
                                        );
                                    }
                                }
                                else
                                {
                                    foreach (var carte in cartesClient)
                                    {
                                        var p1 = new Periode(startPeriod, endPeriod);

                                        var p2 = new Periode(
                                            GetDateCreationCarte(carte)!.Value,
                                            endPeriod
                                        );

                                        // le minimum selectionné entre la période en cours et la carte valide
                                        var minPeriodSelected = MinPeriode(p1, p2);

                                        int duree = Math.Max(
                                            1,
                                            minPeriodSelected != null
                                                ? NombreMois(
                                                    minPeriodSelected.Debut,
                                                    minPeriodSelected.Fin
                                                ) ?? 0
                                                : 0
                                        );

                                        long total = duree * cartePrix[carte.CodeCarte!];

                                        string codeTarifComplet = BuildCodeTarifComplet(
                                            carte.EstActifCodeTarifNumeroCompte,
                                            carte.CodeCarte
                                        );

                                        string DesignationCarte = DesignationCartes(
                                            codeTarifComplet
                                        );

                                        prixMensuelCarte = PrixUnitaireCarte(carte.CodeCarte!);

                                        bkmvtis.Add(
                                            new Bkmvti
                                            {
                                                NumeroCompte = ncpf,
                                                DateCreationCarte = DateTime.SpecifyKind(
                                                    dateCreationCarte!.Value.LocalDateTime,
                                                    DateTimeKind.Utc
                                                ),
                                                DateValiditeCarte = DateTime.SpecifyKind(
                                                    dateValiditeCarte!.Value.LocalDateTime,
                                                    DateTimeKind.Utc
                                                ),
                                                CodeTarif = codeTarifComplet,
                                                CodeCarte = carte.CodeCarte!,
                                                DesignationCarte = DesignationCarte,
                                                startPeriod = DateTime.SpecifyKind(
                                                    startPeriod,
                                                    DateTimeKind.Utc
                                                ),
                                                endPeriod = DateTime.SpecifyKind(
                                                    startPeriod,
                                                    DateTimeKind.Utc
                                                ), // bkPrdCliDto!.ddsou
                                                TypeMag = typeMagResult.Id,
                                                CodeIN = "IN3",
                                                CodeDevise =
                                                    carte.DateValiditeAgenceCodeDeviseNumeroCompte!.Substring(
                                                        9,
                                                        3
                                                    ),
                                                EstActif =
                                                    carte.EstActifCodeTarifNumeroCompte!.Substring(
                                                        0,
                                                        1
                                                    ),
                                                CodeAgence =
                                                    carte.DateValiditeAgenceCodeDeviseNumeroCompte.Substring(
                                                        4,
                                                        5
                                                    ),
                                                TypeBeneficiaire = "AUTO",
                                                ReferenceBeneficiaire = 691228,
                                                CleBeneficiaire = 46,
                                                DatePrelevement = DateTime.SpecifyKind(
                                                    startPeriod,
                                                    DateTimeKind.Utc
                                                ),
                                                PrixUnitCarte = duree * prixMensuelCarte, // prix mensuel de la carte associée ou pas à un pack
                                                ReferenceOperation =
                                                    "RVSA"
                                                    + start.ToString("yy")
                                                    + start.Month.ToString("D2")
                                                    + start.Day.ToString("D2"),
                                                CodeOperation = "C",
                                                CodeEmetteur = "FACSER",
                                                IndicateurDomiciliation = "N",
                                                LibelleCarte = BuildLibelleCarte(
                                                    carte.EstActifCodeTarifNumeroCompte,
                                                    carte.CodeCarte,
                                                    startPeriod
                                                ),
                                                Carte = carte.NumCarte!,
                                                Sequence = "001",
                                            }
                                        );
                                    }
                                }
                            }
                            else
                            {
                                foreach (var carte in cartesClient)
                                {
                                    var p1 = new Periode(startPeriod, endPeriod);

                                    var p2 = new Periode(
                                        GetDateCreationCarte(carte)!.Value,
                                        endPeriod
                                    );

                                    // le minimum selectionné entre la période en cours et la carte valide
                                    var minPeriodSelected = MinPeriode(p1, p2);

                                    int duree = Math.Max(
                                        1,
                                        minPeriodSelected != null
                                            ? NombreMois(
                                                minPeriodSelected.Debut,
                                                minPeriodSelected.Fin
                                            ) ?? 0
                                            : 0
                                    );

                                    long total = duree * cartePrix[carte.CodeCarte!];

                                    string codeTarifComplet = BuildCodeTarifComplet(
                                        carte.EstActifCodeTarifNumeroCompte,
                                        carte.CodeCarte
                                    );

                                    string DesignationCarte = DesignationCartes(codeTarifComplet);

                                    prixMensuelCarte = PrixUnitaireCarte(carte.CodeCarte!);

                                    bkmvtis.Add(
                                        new Bkmvti
                                        {
                                            NumeroCompte = ncpf,
                                            DateCreationCarte = DateTime.SpecifyKind(
                                                dateCreationCarte!.Value.LocalDateTime,
                                                DateTimeKind.Utc
                                            ),
                                            DateValiditeCarte = DateTime.SpecifyKind(
                                                dateValiditeCarte!.Value.LocalDateTime,
                                                DateTimeKind.Utc
                                            ),
                                            CodeTarif = codeTarifComplet,
                                            CodeCarte = carte.CodeCarte!,
                                            DesignationCarte = DesignationCarte,
                                            startPeriod = DateTime.SpecifyKind(
                                                startPeriod,
                                                DateTimeKind.Utc
                                            ),
                                            endPeriod = DateTime.SpecifyKind(
                                                startPeriod,
                                                DateTimeKind.Utc
                                            ), // bkPrdCliDto!.ddsou
                                            TypeMag = typeMagResult.Id,
                                            CodeIN = "IN3",
                                            CodeDevise =
                                                carte.DateValiditeAgenceCodeDeviseNumeroCompte!.Substring(
                                                    9,
                                                    3
                                                ),
                                            EstActif =
                                                carte.EstActifCodeTarifNumeroCompte!.Substring(
                                                    0,
                                                    1
                                                ),
                                            CodeAgence =
                                                carte.DateValiditeAgenceCodeDeviseNumeroCompte.Substring(
                                                    4,
                                                    5
                                                ),
                                            TypeBeneficiaire = "AUTO",
                                            ReferenceBeneficiaire = 691228,
                                            CleBeneficiaire = 46,
                                            DatePrelevement = DateTime.SpecifyKind(
                                                startPeriod,
                                                DateTimeKind.Utc
                                            ),
                                            PrixUnitCarte = duree * prixMensuelCarte, // prix mensuel de la carte associée ou pas à un pack
                                            ReferenceOperation =
                                                "RVSA"
                                                + start.ToString("yy")
                                                + start.Month.ToString("D2")
                                                + start.Day.ToString("D2"),
                                            CodeOperation = "C",
                                            CodeEmetteur = "FACSER",
                                            IndicateurDomiciliation = "N",
                                            LibelleCarte = BuildLibelleCarte(
                                                carte.EstActifCodeTarifNumeroCompte,
                                                carte.CodeCarte,
                                                startPeriod
                                            ),
                                            Carte = carte.NumCarte!,
                                            Sequence = "001",
                                        }
                                    );
                                }
                            }
                        }

                        await _bkmvtiRepository.SaveBkmvtiAsync(bkmvtis);
                        await _emailService.SendEmailAsync(
                            "valdesfeutseu@gmail.com", //_httpContextAccessor.HttpContext?.User?.Identity?.Name!, // ceci correspond à l'adresse exacte de l'utilisateur connecté
                            "Notification MAG",
                            "<h3>Votre traitement est terminé</h3>"
                        );
                        // Si tout a réussi, commit de la transaction
                        await transaction.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        // Rollback si erreur
                        await transaction.RollbackAsync();
                        return new ServiceResult<string>
                        {
                            Success = false,
                            Message = "Erreur lors de la sauvegarde : " + ex.Message,
                        };
                    }
                }

                return new ServiceResult<string>
                {
                    Success = true,
                    Message = "Manque à gagner enregistré avec succès!!!",
                };
            }
            catch (Exception ex)
            {
                _logger.LogInformation("message erreur de traitement. cause : " + ex.Message);
                return new ServiceResult<string>
                {
                    Success = false,
                    Message = "message erreur de traitement. cause : " + ex.Message,
                };
            }
        }

        public string DesignationCartes(string codeTarifComplet)
        {
            return codeTarifNom.TryGetValue(codeTarifComplet, out var designation)
                ? designation
                : "Nom inconnu";
        }

        public long PrixUnitaireCarte(string codeCarte)
        {
            return cartePrix[codeCarte];
        }

        List<CartePackageResult> BuildCartePackageList(
            List<Apprints> cartesClient,
            Dictionary<string, PackagesActifsResponse> packActifs,
            string ncpf
        )
        {
            var result = new List<CartePackageResult>();

            if (!packActifs.TryGetValue(ncpf, out var pack))
                return result;

            foreach (var carte in cartesClient)
            {
                result.Add(
                    new CartePackageResult
                    {
                        CodeCarte = carte.CodeCarte!,
                        CodePackage = pack.cpack,
                    }
                );
            }

            return result;
        }

        Periode MinPeriode(Periode p1, Periode p2)
        {
            // p2 est incluse dans p1 => p2 est le min
            if (p2.Debut >= p1.Debut)
                return p2;
            return p1;
        }

        // Helper: construit la clé complète du tarif (préfixe + code carte)
        private string BuildCodeTarifComplet(
            string? estActifCodeTarifNumeroCompte,
            string? codeCarte
        )
        {
            if (
                string.IsNullOrEmpty(estActifCodeTarifNumeroCompte)
                || string.IsNullOrEmpty(codeCarte)
            )
                return string.Empty;

            string prefix =
                estActifCodeTarifNumeroCompte.Length >= 3
                    ? estActifCodeTarifNumeroCompte.Substring(1, 2)
                    : estActifCodeTarifNumeroCompte;
            return prefix + codeCarte;
        }

        // Helper: construit le libellé sécurisé en vérifiant le dictionnaire
        private string BuildLibelleCarte(
            string? estActifCodeTarifNumeroCompte,
            string? codeCarte,
            DateTime startPeriod
        )
        {
            string key = BuildCodeTarifComplet(estActifCodeTarifNumeroCompte, codeCarte);
            if (string.IsNullOrEmpty(key))
                return "Nom inconnu";

            if (codeTarifNom.TryGetValue(key, out var libelle))
            {
                return $"Ext. {libelle} {startPeriod.ToString("MMM", new CultureInfo("fr-FR"))} {startPeriod.Year}";
            }
            else
            {
                _logger.LogWarning("Libellé tarif introuvable pour la clé: {Key}", key);
                return $"Ext. Nom inconnu {startPeriod.ToString("MMM", new CultureInfo("fr-FR"))} {startPeriod.Year} ";
            }
        }

        public DateTimeOffset? GetDateCreationCarte(Apprints apprint)
        {
            // EXTRACTION DE LA DATE DE CREATION DE LA CARTE
            StringBuilder strbuilderdatecreation = new StringBuilder();
            var dateCreationCarteTransform = apprint.DateCreationCarte;
            strbuilderdatecreation.Append(dateCreationCarteTransform.Substring(4, 2));
            strbuilderdatecreation.Append("/");
            strbuilderdatecreation.Append(dateCreationCarteTransform.Substring(2, 2));
            strbuilderdatecreation.Append("/");
            strbuilderdatecreation.Append(dateCreationCarteTransform.Substring(0, 2));

            string dCreationCarte = strbuilderdatecreation.ToString();

            DateTime? dateCreationCarte = null;

            // Tentative de conversion
            if (DateTime.TryParse(dCreationCarte, out DateTime parsedDate))
            {
                dateCreationCarte = parsedDate;
                return dateCreationCarte;
            }
            else
            {
                _logger.LogInformation("date de création : " + dateCreationCarte);

                return null;
            }
        }

        public DateTimeOffset? getDateValiditeCarte(Apprints apprint)
        {
            // EXTRACTION DE LA DATE DE VALIDITE DE LA CARTE
            var dateValiditeAgenceCodeDeviseNumeroCompte =
                apprint.DateValiditeAgenceCodeDeviseNumeroCompte;
            var annee = int.Parse(dateValiditeAgenceCodeDeviseNumeroCompte!.Substring(0, 2));
            var mois = int.Parse(dateValiditeAgenceCodeDeviseNumeroCompte.Substring(2, 2));
            annee += 2000;
            _logger.LogInformation("date de validite carte : " + mois + "/" + annee);
            // Création de la date avec le 1er jour du mois
            DateTime dateValiditeCarte = new DateTime(annee, mois, 1);
            _logger.LogInformation("date de validite carte : " + dateValiditeCarte);
            return dateValiditeCarte;
        }

        public Apprints ConvertTxtToApprint(string ligne, long numLigne)
        {
            Apprints apprint = new Apprints();

            string[] columns = ligne.Split("\t", StringSplitOptions.RemoveEmptyEntries);

            foreach (string col in columns)
            {
                _logger.LogInformation("numero ligne : " + numLigne + " donnée : " + col);
            }

            return new Apprints
            {
                NumCarte = Convert.ToString(columns[0]),
                NomPropCarte = Convert.ToString(columns[1]),
                LongNum = Convert.ToString(columns[2]),
                VhCodeCarte = Convert.ToString(columns[3]),
                QZero = Convert.ToString(columns[4]),
                DateValiditeAgenceCodeDeviseNumeroCompte = Convert.ToString(columns[5]),
                DateCreationCarte = Convert.ToString(columns[6]),
                EstActifCodeTarifNumeroCompte = Convert.ToString(columns[7]),
                CodeCarte = Convert.ToString(columns[8]),
                NomPrenom = Convert.ToString(columns[9]),
                LastProp = Convert.ToString(columns[10]),
            };
        }

        public byte[] GenerateFile(List<BkmvtiResult> bkmvtis)
        {
            var lignes = bkmvtis.Select(d => BuildLine(d));

            var contenu = string.Join(Environment.NewLine, lignes);

            return Encoding.UTF8.GetBytes(contenu);
        }

        public async Task<bool> IsDownloadAsync(Guid typeMagId)
        {
            return await _typeMagRepository.IsDownload(typeMagId);
        }

        private string BuildLine(BkmvtiResult bkmvtis)
        {
            var bkmvti = new List<string>
            {
                bkmvtis.CodeAgence, // 04000
                "001", // Sequence
                "345100",
                bkmvtis.NumeroCompte,
                " ",
                "IN3",
                " ",
                " ",
                "AUTO",
                "691228", // ReferenceBeneficiaire
                "46", // CleBeneficiaire
                bkmvtis.DatePrelevement.ToString("dd/MM/yyyy"),
                " ",
                bkmvtis.DatePrelevement.ToString("dd/MM/yyyy"),
                bkmvtis.Total.ToString(),
                "C", // CodeOperation
                bkmvtis.LibelleCarte, // Exple : Ext. Horizon jan. 2025/ Redev Horizon avr. 2025
                "N", // indicateur de domicilisation
                $"RVSA{bkmvtis.DatePrelevement.ToString("yyMMdd")}",
                // compléter jusqu’à ton format exact
                " ",
                " ",
                " ",
                " ",
                " ",
                " ",
                " ",
                " ",
                " ",
                " ",
                " ",
                " ",
                " ",
                " ",
                " ",
                " ",
                " ",
                " ",
                " ",
                bkmvtis.CodeAgence,
                " ",
                " ",
                "001",
                " ",
                " ",
                bkmvtis.CodeAgence + "C",
                " ",
                "001",
                " ",
                " ",
                " ",
                " ",
                " ",
                "FACSER",
                " ",
                " ",
                " ",
                " ",
                " ",
                " ",
            };

            return string.Join("|", bkmvti);
        }

        int? NombreMois(DateTimeOffset? dateDebut, DateTimeOffset? dateFin)
        {
            return (dateFin?.Year - dateDebut?.Year) * 12 + (dateFin?.Month - dateDebut?.Month);
        }

        public int CalculerIntersection(
            DateTime? debutA,
            DateTime? finA,
            DateTime? debutB,
            DateTime? finB
        )
        {
            if (!debutA.HasValue || !finA.HasValue || !debutB.HasValue || !finB.HasValue)
                return 0; // Pas de calcul si une date est manquante

            DateTime debutIntersection = debutA.Value > debutB.Value ? debutA.Value : debutB.Value;
            DateTime finIntersection = finA.Value < finB.Value ? finA.Value : finB.Value;

            if (debutIntersection > finIntersection)
                return 0;

            int diffMois =
                (finIntersection.Year - debutIntersection.Year) * 12
                + (finIntersection.Month - debutIntersection.Month);

            // On compte le mois en cours si les jours se chevauchent
            if (finIntersection.Day >= debutIntersection.Day)
                diffMois++;

            return diffMois;
        }

        public Dictionary<string, ComptesActifsResponse> GetComptesActifs(
            ExcelWorksheet worksheetCompteActif
        )
        {
            var comptesActif = new Dictionary<string, ComptesActifsResponse>();
            // commencer par la ligne 2 si la ligne 1 est l'en-tête
            for (int row = 2; row <= worksheetCompteActif.Dimension.End.Row; row++)
            {
                string ncp = worksheetCompteActif.Cells[row, 1].Text.Trim();
                _logger.LogInformation(
                    $"Max Row worksheetCompteActif : {worksheetCompteActif.Dimension?.End.Row}"
                );
                _logger.LogInformation($"Max Col: {worksheetCompteActif.Dimension?.End.Column}");
                if (!string.IsNullOrEmpty(ncp))
                {
                    comptesActif[ncp] = new ComptesActifsResponse { ncp = ncp };
                }
            }
            return comptesActif;
        }

        public Dictionary<string, ComptesOuvertsResponse> GetComptesOuvert(
            ExcelWorksheet worksheetCompteOuvert
        )
        {
            var comptesOuvert = new Dictionary<string, ComptesOuvertsResponse>();
            // commencer par la ligne 2 si la ligne 1 est l'en-tête
            for (int row = 2; row <= worksheetCompteOuvert.Dimension.End.Row; row++)
            {
                _logger.LogInformation(
                    $"Max Row worksheetCompteOuvert: {worksheetCompteOuvert.Dimension?.End.Row}"
                );
                _logger.LogInformation($"Max Col: {worksheetCompteOuvert.Dimension?.End.Column}");
                string ncp = worksheetCompteOuvert.Cells[row, 1].Text.Trim();
                if (!string.IsNullOrEmpty(ncp))
                {
                    comptesOuvert[ncp] = new ComptesOuvertsResponse
                    {
                        ncp = ncp,
                        cfe = worksheetCompteOuvert.Cells[row, 2].Text.Trim(),
                    };
                }
            }
            return comptesOuvert;
        }

        public Dictionary<string, DateDsouPackEchuResponse> GetDsouPackEchu(
            ExcelWorksheet worksheetDsouPackEchu
        )
        {
            var dateDsouPackEchu = new Dictionary<string, DateDsouPackEchuResponse>();
            // commencer par la ligne 2 si la ligne 1 est l'en-tête
            for (int row = 2; row <= worksheetDsouPackEchu.Dimension.End.Row; row++)
            {
                _logger.LogInformation(
                    $"Max Row worksheetDsouPackEchu : {worksheetDsouPackEchu.Dimension?.End.Row}"
                );
                _logger.LogInformation($"Max Col: {worksheetDsouPackEchu.Dimension?.End.Column}");
                string ncpf = worksheetDsouPackEchu.Cells[row, 1].Text.Trim();

                if (
                    !string.IsNullOrEmpty(ncpf)
                    && DateTime.TryParse(
                        worksheetDsouPackEchu.Cells[row, 2].Value?.ToString(),
                        out DateTime date
                    )
                )
                {
                    dateDsouPackEchu[ncpf] = new DateDsouPackEchuResponse
                    {
                        ncpf = ncpf,
                        maxDsouf = date,
                    };
                }
            }
            return dateDsouPackEchu;
        }

        public Dictionary<string, HistCptDebiteRedevCarteResponse> GetHistCptDebiteRedevCarte(
            ExcelWorksheet worksheetHistCptDebiteRedev
        )
        {
            var histCptDebiteRedevCarte = new Dictionary<string, HistCptDebiteRedevCarteResponse>();
            // commencer par la ligne 2 si la ligne 1 est l'en-tête
            for (int row = 2; row <= worksheetHistCptDebiteRedev.Dimension.End.Row; row++)
            {
                _logger.LogInformation(
                    $"Max Row worksheetHistCptDebiteRedev : {worksheetHistCptDebiteRedev.Dimension?.End.Row}"
                );
                _logger.LogInformation(
                    $"Max Col: {worksheetHistCptDebiteRedev.Dimension?.End.Column}"
                );
                string ncp = worksheetHistCptDebiteRedev.Cells[row, 1].Text.Trim();
                if (!string.IsNullOrEmpty(ncp))
                {
                    histCptDebiteRedevCarte[ncp] = new HistCptDebiteRedevCarteResponse
                    {
                        ncp = ncp,
                        SumMon = long.Parse(worksheetHistCptDebiteRedev.Cells[row, 2].Text.Trim()),
                    };
                }
            }
            return histCptDebiteRedevCarte;
        }

        public Dictionary<string, PackagesActifsResponse> GetPackagesActifs(
            ExcelWorksheet worksheetPackActif
        )
        {
            var packActif = new Dictionary<string, PackagesActifsResponse>();
            // commencer par la ligne 2 si la ligne 1 est l'en-tête
            for (int row = 2; row <= worksheetPackActif.Dimension.End.Row; row++)
            {
                _logger.LogInformation(
                    $"Max Row worksheetPackActif : {worksheetPackActif.Dimension?.End.Row}"
                );
                _logger.LogInformation($"Max Col: {worksheetPackActif.Dimension?.End.Column}");
                string ncpf = worksheetPackActif.Cells[row, 1].Text.Trim();
                if (!string.IsNullOrEmpty(ncpf))
                {
                    packActif[ncpf] = new PackagesActifsResponse
                    {
                        ncpf = ncpf,
                        cpack = worksheetPackActif.Cells[row, 2].Text.Trim(),
                        lib = worksheetPackActif.Cells[row, 3].Text.Trim(),
                    };
                }
            }
            return packActif;
        }

        // création du fichier excel
        //public byte[] TxtToExcel(List<Apprints> apprints, DateTime DateDebut, DateTime DateFin)
        //{
        //    try
        //    {
        //        using var package = new ExcelPackage();
        //        using var worksheet = package.Workbook.Worksheets.Add("generate BKMBTI file");

        //        // conversion de la date en mois uniquement
        //        string moisDebut = DateDebut.ToString("MMMM", new CultureInfo("fr-FR"));
        //        string moisFin = DateFin.ToString("MMMM", new CultureInfo("fr-FR"));

        //        var nbrMoisPasse = NombreMois(DateDebut, DateFin);

        //        // creation de l'en-tete du fichier excel
        //        worksheet.Cells[1, 1].Value = "Carte";
        //        worksheet.Cells[1, 2].Value = "Date validité carte";
        //        worksheet.Cells[1, 3].Value = "Numéro compte";
        //        worksheet.Cells[1, 4].Value = "Colonne1";
        //        worksheet.Cells[1, 5].Value = "Date création carte";
        //        worksheet.Cells[1, 6].Value = "Code tarif";
        //        worksheet.Cells[1, 7].Value = "Code carte";
        //        worksheet.Cells[1, 8].Value = "Nom carte";
        //        worksheet.Cells[1, 9].Value = "Prix unit carte";
        //        worksheet.Cells[1, 10].Value = "Max(date creation carte, dfsou package)";
        //        worksheet.Cells[1, 11].Value = $"MAG {moisDebut}-{moisFin} 2025 (Calcul)";
        //        worksheet.Cells[1, 12].Value = $"MAG {moisDebut}-{moisFin} 2025_Final";
        //        worksheet.Cells[1, 13].Value = $"MAG {moisDebut}-{moisFin} / PU";
        //        worksheet.Cells[1, 14].Value = $"MAG {moisDebut}-{moisFin}";
        //        worksheet.Cells[1, 15].Value = "Nom embossé";
        //        worksheet.Cells[1, 16].Value = "Compte fermés";
        //        worksheet.Cells[1, 17].Value = "Packages actifs rattachés";
        //        worksheet.Cells[1, 18].Value = "Actif/Inactif";
        //        worksheet.Cells[1, 19].Value = "Déjà payé entre Jan et Mai";
        //        worksheet.Cells[1, 20].Value = "Date dernière souscription packages échus";
        //        worksheet.Cells[1, 21].Value = "Déjà débités entre Jan et Mai";
        //        worksheet.Cells[1, 22].Value = "Appréciation des débits cumulés cartes";
        //        worksheet.Cells[1, 23].Value = "Min(DCO)";
        //        worksheet.Cells[1, 24].Value = "Max(DCO)";
        //        worksheet.Cells[1, 25].Value = "Date fin souscription packages échus";

        //        // formatage des colonnes 10, 20, 25 en date sinon EPPLUS Excel renverai une autre valeur
        //        worksheet.Column(10).Style.Numberformat.Format = "dd/MM/yyyy";
        //        worksheet.Column(20).Style.Numberformat.Format = "dd/MM/yyyy";
        //        worksheet.Column(25).Style.Numberformat.Format = "dd/MM/yyyy";

        //        int row = 2;

        //        foreach (Apprints apprint in apprints)
        //        {

        //            // extraction date validite
        //            StringBuilder strBuilderDateValidite = new StringBuilder();
        //            var dateValiditeAgenceCodeDeviseNumeroCompte = apprint.DateValiditeAgenceCodeDeviseNumeroCompte;
        //            strBuilderDateValidite.Append(dateValiditeAgenceCodeDeviseNumeroCompte!.Substring(2, 2));
        //            strBuilderDateValidite.Append("/");
        //            strBuilderDateValidite.Append(dateValiditeAgenceCodeDeviseNumeroCompte.Substring(0, 2));

        //            var dateValidite = strBuilderDateValidite.ToString();

        //            // extraction date creation carte
        //            StringBuilder strbuilderdatecreation = new StringBuilder();
        //            var dateCreationCarteTransform = apprint.DateCreationCarte;
        //            strbuilderdatecreation.Append(dateCreationCarteTransform.Substring(4, 2));
        //            strbuilderdatecreation.Append("/");
        //            strbuilderdatecreation.Append(dateCreationCarteTransform.Substring(2, 2));
        //            strbuilderdatecreation.Append("/");
        //            strbuilderdatecreation.Append(dateCreationCarteTransform.Substring(0, 2));

        //            string dateCreationCarte = strbuilderdatecreation.ToString();

        //            var agence = apprint.DateValiditeAgenceCodeDeviseNumeroCompte!.Substring(4, 5);
        //            var codeDevise = apprint.DateValiditeAgenceCodeDeviseNumeroCompte.Substring(9, 3);
        //            var numeroCompte = apprint.DateValiditeAgenceCodeDeviseNumeroCompte.Substring(12);
        //            // extraction client actif
        //            var estActif = apprint.EstActifCodeTarifNumeroCompte!.Substring(0, 1);
        //            // extraction code tarif
        //            var codeTarif = apprint.EstActifCodeTarifNumeroCompte.Substring(1, 2);

        //            // recuperation du non de la carte
        //            StringBuilder strBNomCarte = new StringBuilder();
        //            strBNomCarte.Append(codeTarif);
        //            strBNomCarte.Append(apprint.CodeCarte);
        //            var nomCarte = strBNomCarte.ToString();

        //            worksheet.Cells[row, 1].Value = apprint.NumCarte; // numero de la carte
        //            worksheet.Cells[row, 2].Value = dateValidite; // date de validite de la carte
        //            worksheet.Cells[row, 3].Value = numeroCompte; // numero de compte du client
        //            worksheet.Cells[row, 4].Value = "";
        //            worksheet.Cells[row, 5].Value = dateCreationCarte;
        //            worksheet.Cells[row, 6].Value = codeTarif;
        //            worksheet.Cells[row, 7].Value = apprint.CodeCarte;

        //            //_logger.LogInformation($"CodeCarte : {apprint.CodeCarte} ");
        //            //_logger.LogInformation($"NumCarte : {apprint.NumCarte} ");
        //            //_logger.LogInformation($"dateValidite : {dateValidite} ");
        //            //_logger.LogInformation($"numeroCompte : {numeroCompte} ");
        //            //_logger.LogInformation($"dateCreationCarte : {dateCreationCarte} ");
        //            //_logger.LogInformation($"codeTarif : {codeTarif} ");
        //            //_logger.LogInformation($"codeTarifNom : {codeTarifNom["C3016"]} ");
        //            // Parse dateValidite safely

        //            // insertion du nom de la carte...

        //            switch (nomCarte)
        //            {
        //                case "CL012":
        //                    worksheet.Cells[row, 8].Value = codeTarifNom["CL012"]; // nom code-tarif/code-carte
        //                    break;
        //                case "CL014":
        //                    worksheet.Cells[row, 8].Value = codeTarifNom["CL014"];
        //                    break;
        //                case "CL015":
        //                    worksheet.Cells[row, 8].Value = codeTarifNom["CL015"];
        //                    break;
        //                case "CL006":
        //                    worksheet.Cells[row, 8].Value = codeTarifNom["CL006"];
        //                    break;
        //                case "CL007":
        //                    worksheet.Cells[row, 8].Value = codeTarifNom["CL007"];
        //                    break;

        //                case "PR011":
        //                    worksheet.Cells[row, 8].Value = codeTarifNom["PR011"];
        //                    break;
        //                case "PR012":
        //                    worksheet.Cells[row, 8].Value = codeTarifNom["PR012"];
        //                    break;
        //                case "PR013":
        //                    worksheet.Cells[row, 8].Value = codeTarifNom["PR013"];
        //                    break;
        //                case "PR016":
        //                    worksheet.Cells[row, 8].Value = codeTarifNom["PR016"];
        //                    break;
        //                case "PR005":
        //                    worksheet.Cells[row, 8].Value = codeTarifNom["PR005"];
        //                    break;
        //                case "PR006":
        //                    worksheet.Cells[row, 8].Value = codeTarifNom["PR006"];
        //                    break;
        //                case "PR007":
        //                    worksheet.Cells[row, 8].Value = codeTarifNom["PR007"];
        //                    break;

        //                case "C3011":
        //                    worksheet.Cells[row, 8].Value = codeTarifNom["C3011"];
        //                    break;
        //                case "C3012":
        //                    worksheet.Cells[row, 8].Value = codeTarifNom["C3012"];
        //                    break;
        //                case "C3016":
        //                    worksheet.Cells[row, 8].Value = codeTarifNom["C3016"];
        //                    break;
        //                case "C3006":
        //                    worksheet.Cells[row, 8].Value = codeTarifNom["C3006"];
        //                    break;
        //                case "C3007":
        //                    worksheet.Cells[row, 8].Value = codeTarifNom["C3007"];
        //                    break;

        //                case "EX011":
        //                    worksheet.Cells[row, 8].Value = codeTarifNom["EX011"];
        //                    break;
        //                case "EX012":
        //                    worksheet.Cells[row, 8].Value = codeTarifNom["EX012"];
        //                    break;
        //                case "EX013":
        //                    worksheet.Cells[row, 8].Value = codeTarifNom["EX013"];
        //                    break;
        //                case "EX016":
        //                    worksheet.Cells[row, 8].Value = codeTarifNom["EX016"];
        //                    break;
        //                case "EX005":
        //                    worksheet.Cells[row, 8].Value = codeTarifNom["EX005"];
        //                    break;
        //                case "EX006":
        //                    worksheet.Cells[row, 8].Value = codeTarifNom["EX006"];
        //                    break;
        //                case "EX007":
        //                    worksheet.Cells[row, 8].Value = codeTarifNom["EX007"];
        //                    break;

        //                default:
        //                    worksheet.Cells[row, 8].Value = "Nom inconnu";
        //                    break;
        //            }

        //            // insertion du code de la carte
        //            switch (apprint.CodeCarte)
        //            {
        //                case "007":
        //                    worksheet.Cells[row, 9].Value = cartePrix["007"]; // cotisations mensuells carte
        //                    worksheet.Cells[row, 11].Value = cartePrix["007"] * nbrMoisPasse;
        //                    worksheet.Cells[row, 12].Value = cartePrix["007"] * nbrMoisPasse;
        //                    worksheet.Cells[row, 13].Value = cartePrix["007"] / cartePrix["007"];
        //                    worksheet.Cells[row, 14].Value = cartePrix["007"];
        //                    break;
        //                case "012":
        //                    worksheet.Cells[row, 9].Value = cartePrix["012"];
        //                    worksheet.Cells[row, 11].Value = cartePrix["012"] * nbrMoisPasse;
        //                    worksheet.Cells[row, 12].Value = cartePrix["012"] * nbrMoisPasse;
        //                    worksheet.Cells[row, 13].Value = cartePrix["012"] / cartePrix["012"];
        //                    worksheet.Cells[row, 14].Value = cartePrix["012"];
        //                    break;
        //                case "006":
        //                    worksheet.Cells[row, 9].Value = cartePrix["006"];
        //                    worksheet.Cells[row, 11].Value = cartePrix["006"] * nbrMoisPasse;
        //                    worksheet.Cells[row, 12].Value = cartePrix["006"] * nbrMoisPasse;
        //                    worksheet.Cells[row, 13].Value = cartePrix["006"] / cartePrix["006"];
        //                    worksheet.Cells[row, 14].Value = cartePrix["006"];
        //                    break;
        //                case "011":
        //                    worksheet.Cells[row, 9].Value = cartePrix["011"];
        //                    worksheet.Cells[row, 11].Value = cartePrix["011"] * nbrMoisPasse;
        //                    worksheet.Cells[row, 12].Value = cartePrix["011"] * nbrMoisPasse;
        //                    worksheet.Cells[row, 13].Value = cartePrix["011"] / cartePrix["011"];
        //                    worksheet.Cells[row, 14].Value = cartePrix["011"];
        //                    break;
        //                case "016":
        //                    worksheet.Cells[row, 9].Value = cartePrix["016"];
        //                    worksheet.Cells[row, 11].Value = cartePrix["016"] * nbrMoisPasse;
        //                    worksheet.Cells[row, 12].Value = cartePrix["016"] * nbrMoisPasse;
        //                    worksheet.Cells[row, 13].Value = cartePrix["016"] / cartePrix["016"];
        //                    worksheet.Cells[row, 14].Value = cartePrix["016"];
        //                    break;
        //                default:
        //                    worksheet.Cells[row, 9].Value = "Code inconnu";
        //                    worksheet.Cells[row, 9].Value = cartePrix["Code inconnu"];
        //                    worksheet.Cells[row, 11].Value = cartePrix["Code inconnu"];
        //                    worksheet.Cells[row, 12].Value = cartePrix["Code inconnu"];
        //                    worksheet.Cells[row, 14].Value = cartePrix["Code inconnu"];
        //                    break;
        //            }

        //            if (dateDersouPackEchus.ContainsKey(numeroCompte))
        //            {

        //                // Get the max date between dateValidite and maxDsouf
        //                DateTime maxDate = (DateTime.Parse(dateCreationCarte) > dateDersouPackEchus[numeroCompte].maxDsouf)
        //                    ? DateTime.Parse(dateCreationCarte)
        //                    : dateDersouPackEchus[numeroCompte].maxDsouf;

        //                worksheet.Cells[row, 10].Value = maxDate;  // MaX (date creation carte, dfsou package)
        //                _logger.LogInformation($"maxdate : {maxDate}");
        //            }

        //            // Logging
        //            _logger.LogInformation($"date derniere : {(dateDersouPackEchus.ContainsKey(numeroCompte) ? dateDersouPackEchus[numeroCompte].maxDsouf : (DateTime?)null)}");
        //            _logger.LogInformation($"date fin souscription : {(dateDersouPackEchus.ContainsKey(numeroCompte) ? dateDersouPackEchus[numeroCompte].maxDsouf : (DateTime?)null)}");

        //            worksheet.Cells[row, 15].Value = apprint.NomPropCarte; // nom proprietaire carte
        //            worksheet.Cells[row, 16].Value = estActif;
        //            worksheet.Cells[row, 17].Value = packActifs.ContainsKey(numeroCompte) ? packActifs[numeroCompte].cpack + " => " + packActifs[numeroCompte].lib : "#N/A";
        //            worksheet.Cells[row, 18].Value = estActif == "N" ? "Actif" : "#N/A";
        //            worksheet.Cells[row, 19].Value = "#N/A";
        //            //
        //            worksheet.Cells[row, 20].Value = dateDersouPackEchus.ContainsKey(numeroCompte)
        //                ? dateDersouPackEchus[numeroCompte].maxDsouf
        //                : (DateTime?)null; // or another default value
        //            worksheet.Cells[row, 21].Value = "#N/A";
        //            worksheet.Cells[row, 22].Value = "#N/A";
        //            worksheet.Cells[row, 23].Value = "#N/A";
        //            worksheet.Cells[row, 24].Value = "#N/A";
        //            // ussage du dsou si le code se trouve dans le dictionnaire
        //            worksheet.Cells[row, 25].Value = dateDersouPackEchus.ContainsKey(numeroCompte)
        //                ? dateDersouPackEchus[numeroCompte].maxDsouf
        //                : (DateTime?)null; // or another default value
        //            row++;

        //        }

        //        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

        //        worksheet.Cells[1, 1, 1, 25].Style.Font.Bold = true;
        //        var headerRange = worksheet.Cells[1, 1, 1, 25];
        //        headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid; // Style de remplissage
        //        headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.YellowGreen); // Couleur de fond CodeBrix.Imaging.Color.LightBlue
        //        headerRange.Style.Font.Bold = true; // Mettre en gras
        //        headerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center; // Centrer le texte

        //        return package.GetAsByteArray();
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.Error.WriteLine($"Erreur lors de la lecture du fichier : {ex.Message}");
        //        throw new NotImplementedException();
        //    }
        //}

        public byte[] TxtToBkmvti(List<Apprints> apprints, DateTime DateDebut, DateTime DateFin)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<TypeMag>> GetAllTypeMagsAsync()
        {
            return await _typeMagRepository.getAllMag();
        }

        public async Task<TypeMagWithSyntheseDto> GetTypeMagWithSynthese(Guid typeMagId)
        {
            return await _typeMagRepository.GetTypeMagWithSyntheseAsync(typeMagId);
        }

        public byte[] TxtToExcel(List<Apprints> apprints, DateTime DateDebut, DateTime DateFin)
        {
            throw new NotImplementedException();
        }
    }
}
