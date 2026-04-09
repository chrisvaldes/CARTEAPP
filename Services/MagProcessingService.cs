using OfficeOpenXml;
using SYSGES_MAGs.Data;
using SYSGES_MAGs.Models;
using SYSGES_MAGs.Models.ModelsDto;
using SYSGES_MAGs.Repository;
using SYSGES_MAGs.Repository.IRepository;
using SYSGES_MAGs.Services.IServices;
using System.Globalization;
using System.Text;

 

namespace SYSGES_MAGs.Services
{
    public class MagProcessingService : IMagProcessingService
    {
        private readonly ILogger<MagProcessingService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IBkPrdCliRepository _bkPrdCliRepository;
        private readonly ITypeMagRepository _typeMagRepository;
        private readonly IBkmvtiRepository _bkmvtiRepository;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ApplicationDbContext _dbContext;
        public BkPrdCliDto bkPrdCliDto = new BkPrdCliDto();
        public long prixMensuelCarte = 0;

        // Injection via constructeur
        public MagProcessingService(
            ILogger<MagProcessingService> logger,
            IHttpContextAccessor httpContextAccessor, IBkPrdCliRepository bkPrdCliRepository, ITypeMagRepository typeMagRepository,
            IBkmvtiRepository kmvtiRepository, IServiceScopeFactory serviceScopeFactory, ApplicationDbContext context)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _bkPrdCliRepository = bkPrdCliRepository;
            _typeMagRepository = typeMagRepository;
            _bkmvtiRepository = kmvtiRepository;
            _serviceScopeFactory = serviceScopeFactory;
            _dbContext = context;
        }

        Dictionary<string, ComptesActifsResponse> comptesActifs = new Dictionary<string, ComptesActifsResponse>();
        Dictionary<string, ComptesOuvertsResponse> comptesOuverts = new Dictionary<string, ComptesOuvertsResponse>();
        Dictionary<string, DateDsouPackEchuResponse> dateDersouPackEchus = new Dictionary<string, DateDsouPackEchuResponse>();
        Dictionary<string, HistCptDebiteRedevCarteResponse> histCptDebiteRedevCartes = new Dictionary<string, HistCptDebiteRedevCarteResponse>();
        Dictionary<string, PackagesActifsResponse> packActifs = new Dictionary<string, PackagesActifsResponse>();

        Dictionary<string, string> codeTarifNom = new Dictionary<string, string>()
        {
            {"CL012", "CARTE HORIZON" },
            {"CL011", "VISA PLATINUM" },
            {"CL014", "VISA FREE" },
            {"CL015", "VISA YOUNG" },
            {"CL006", "VISA PREMIER" },
            {"CL007", "VISA CLASSIC" },

            {"PR011", "VISA PLATINUM" },
            {"PR012", "CARTE HORIZON" },
            {"PR013", "VISA HORIZON PREPAYEE" },
            {"PR016", "VISA INFINITE" },
            {"PR005", "VISA BUSINESS" },
            {"PR006", "VISA PREMIER" },
            {"PR007", "VISA CLASSIC" },

            {"C3011", "CARTE PLATINUM" },
            {"C3012", "CARTE HORIZON" },
            {"C3016", "CARTE INFINITE" },
            {"C3006", "VISA PREMIER" },
            {"C3007", "VISA CLASSIC" },

            {"EX011", "CARTE PLATINUM" },
            {"EX012", "CARTE HORIZON" },
            {"EX013", "VISA HORIZON PREPAYEE" },
            {"EX016", "CARTE INFINITE" },
            {"EX005", "VISA BUSINNESS" },
            {"EX006", "VISA PREMIER" },
            {"EX007", "VISA CLASSIC" },
        };

        Dictionary<string, long> cartePrix = new Dictionary<string, long>
        {
            {"012", 2504 },
            {"007", 4770 },
            {"006", 8705 },
            {"011", 14906 },
            {"016", 29813 },
        };

        // exlusion des cartes visa free, young, businnes
        string[] cartesExclu = new[] { "014", "015", "005" };


        public async Task<ServiceResult<string>> ProcessTxtExcelFiles(IFormFile apprintFile, IFormFile openAccountFile, IFormFile activeAccount, IFormFile dateLastSouPackEchuFile, IFormFile activePackageFile, IFormFile accountHisDebiteByRedevCardFile, string typeMag, DateTime startPeriod, DateTime endPeriod)
        {
            // creation d'une liste d'appring
            List<Apprints> apprints = new List<Apprints>();
            List<Bkmvti> bkmvtis = new List<Bkmvti>();
            DateTime dateDebutExecution = DateTime.Now;
            long ligne = 0;

            _logger.LogInformation("typemag : " + typeMag);


            try
            {

                var dureePeriode = NombreMois(startPeriod, endPeriod);

                if((startPeriod > endPeriod) || (dureePeriode < 1))
                {
                    return new ServiceResult<string>
                    {
                        Success = false,
                        Message = "La période de début doit être inférieur à la période de fin, et l'écart >= 1",

                    };
                }

                TypeMag? existTypeMag = await _typeMagRepository.IsTypeMagExist(startPeriod);

                if(existTypeMag != null)
                {
                    return new ServiceResult<string>
                    {
                        Success = false,
                        Message = "Manque à gagner déjà récupérer sur la période.",

                    };
                }
 

                _logger.LogInformation("dans le try : " + typeMag);
                // lecture du fichier apprint 
                using var reader = new StreamReader(apprintFile.OpenReadStream());

                DateTime start;
                DateTime end;

                _logger.LogInformation("le fichier n'est pas vide : ");

                // SI LE FICHIER CONTIENT DES DONNEES, ENREGISTREZ LE TYPE DE MAG DANS LA BASE DE DONNEES
                if (DateTime.TryParse(startPeriod.ToString(), out DateTime startPer) && DateTime.TryParse(endPeriod.ToString(), out DateTime endPer))
                {
                    start = startPer;
                    end = endPer;

                    using var transaction = await _dbContext.Database.BeginTransactionAsync();
                    try
                    {
                        // sauvegarde du type de manque à gagner effectué
                        TypeMag typeMagResult = await _typeMagRepository.SaveTypeMagAsync(new TypeMag
                        {
                            Description = "cap_" + start.Day.ToString("D2") + "_" + start.ToString("MMM", new CultureInfo("fr-FR"))
                              + "_" + end.Day.ToString("D2") + "_" + end.ToString("MMM", new CultureInfo("fr-FR"))
                              + "_" + end.Year,
                            Email = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "unknown",
                            PeriodeDebut = DateTime.SpecifyKind(startPeriod, DateTimeKind.Utc),
                            PeriodeFin = DateTime.SpecifyKind(endPeriod, DateTimeKind.Utc),
                        });
                        _logger.LogInformation("type mag :  " + typeMagResult.Id);

                        // ligne d'en-tête 
                        string header = await reader.ReadLineAsync() ?? string.Empty;

                        _logger.LogInformation("header :  " + header);

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

                                // retour d'une ligne convertie du fichier apprint en un enregistrement Apprints
                                Apprints apprint = ConvertTxtToApprint(ligneApprint, ligne);

                                // EXTRACTION DU NUMERO DE COMPTE ET DES DATE DE CREATION ET DE VALIDATION DE LA CARTE

                                // extraction du numéro de compte
                                var numeroCompte = apprint.DateValiditeAgenceCodeDeviseNumeroCompte!.Substring(12);
                                _logger.LogInformation("************** numero de compte : " + numeroCompte);
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
                                }

                                _logger.LogInformation("date de création : " + dateCreationCarte);


                                // EXTRACTION DE LA DATE DE VALIDITE DE LA CARTE
                                var dateValiditeAgenceCodeDeviseNumeroCompte = apprint.DateValiditeAgenceCodeDeviseNumeroCompte;
                                var annee = int.Parse(dateValiditeAgenceCodeDeviseNumeroCompte!.Substring(0, 2));
                                var mois = int.Parse(dateValiditeAgenceCodeDeviseNumeroCompte.Substring(2, 2));
                                annee += 2000;

                                _logger.LogInformation("date de validite carte : " + mois + "/" + annee);


                                // Création de la date avec le 1er jour du mois
                                DateTime dateValiditeCarte = new DateTime(annee, mois, 1);

                                _logger.LogInformation("date de validite carte : " + dateValiditeCarte);


                                // Comparaison avec les dates fournies et autres exclusions
                                if ((dateCreationCarte.HasValue && dateCreationCarte!.Value <= DateTime.UtcNow) ||
                                    (!comptesOuverts.ContainsKey(numeroCompte)) ||
                                    (!packActifs.ContainsKey(numeroCompte)) ||
                                    (apprint.CodeCarte != null && cartesExclu.Contains(apprint.CodeCarte)) ||
                                    ((!numeroCompte.StartsWith("02")) &&
                                    (!numeroCompte.StartsWith("03"))))
                                {
                                    _logger.LogInformation($"Ligne {ligne} : Carte invalide");

                                    // Utiliser CodeTarif = prefix + codeCarte comme précédemment
                                    string codeTarifComplet = BuildCodeTarifComplet(apprint.EstActifCodeTarifNumeroCompte, apprint.CodeCarte);

                                    string DesignationCarte = "";

                                    switch (codeTarifComplet)
                                    {
                                        case "CL012":
                                            DesignationCarte = codeTarifNom["CL012"]; // nom code-tarif/code-carte
                                            break;
                                        case "CL014":
                                            DesignationCarte = codeTarifNom["CL014"];
                                            break;
                                        case "CL015":
                                            DesignationCarte = codeTarifNom["CL015"];
                                            break;
                                        case "CL006":
                                            DesignationCarte = codeTarifNom["CL006"];
                                            break;
                                        case "CL007":
                                            DesignationCarte = codeTarifNom["CL007"];
                                            break;

                                        case "PR011":
                                            DesignationCarte = codeTarifNom["PR011"];
                                            break;
                                        case "PR012":
                                            DesignationCarte = codeTarifNom["PR012"];
                                            break;
                                        case "PR013":
                                            DesignationCarte = codeTarifNom["PR013"];
                                            break;
                                        case "PR016":
                                            DesignationCarte = codeTarifNom["PR016"];
                                            break;
                                        case "PR005":
                                            DesignationCarte = codeTarifNom["PR005"];
                                            break;
                                        case "PR006":
                                            DesignationCarte = codeTarifNom["PR006"];
                                            break;
                                        case "PR007":
                                            DesignationCarte = codeTarifNom["PR007"];
                                            break;

                                        case "C3011":
                                            DesignationCarte = codeTarifNom["C3011"];
                                            break;
                                        case "C3012":
                                            DesignationCarte = codeTarifNom["C3012"];
                                            break;
                                        case "C3016":
                                            DesignationCarte = codeTarifNom["C3016"];
                                            break;
                                        case "C3006":
                                            DesignationCarte = codeTarifNom["C3006"];
                                            break;
                                        case "C3007":
                                            DesignationCarte = codeTarifNom["C3007"];
                                            break;

                                        case "EX011":
                                            DesignationCarte = codeTarifNom["EX011"];
                                            break;
                                        case "EX012":
                                            DesignationCarte = codeTarifNom["EX012"];
                                            break;
                                        case "EX013":
                                            DesignationCarte = codeTarifNom["EX013"];
                                            break;
                                        case "EX016":
                                            DesignationCarte = codeTarifNom["EX016"];
                                            break;
                                        case "EX005":
                                            DesignationCarte = codeTarifNom["EX005"];
                                            break;
                                        case "EX006":
                                            DesignationCarte = codeTarifNom["EX006"];
                                            break;
                                        case "EX007":
                                            DesignationCarte = codeTarifNom["EX007"];
                                            break;

                                        default:
                                            DesignationCarte = "Nom inconnu";
                                            break;
                                    }

                                    switch (apprint.CodeCarte)
                                    {
                                        case "007":
                                            prixMensuelCarte = cartePrix["007"];
                                            break;
                                        case "012":
                                            prixMensuelCarte = cartePrix["012"]; 
                                            break;
                                        case "006":
                                            prixMensuelCarte = cartePrix["006"]; 
                                            break;
                                        case "011":
                                            prixMensuelCarte = cartePrix["011"]; 
                                            break;
                                        case "016":
                                            prixMensuelCarte = cartePrix["016"]; 
                                            break;
                                        default: 
                                            prixMensuelCarte = 0; 
                                            break;
                                    }

                                    bkmvtis.Add(new Bkmvti
                                    {
                                        NumeroCompte = numeroCompte,
                                        DateCreationCarte = DateTime.SpecifyKind(dateCreationCarte!.Value, DateTimeKind.Utc),
                                        DateValiditeCarte = DateTime.SpecifyKind(dateValiditeCarte, DateTimeKind.Utc),
                                        CodeTarif = codeTarifComplet,
                                        CodeCarte = apprint.CodeCarte!,
                                        DesignationCarte = DesignationCarte,
                                        startPeriod = DateTime.SpecifyKind(startPeriod, DateTimeKind.Utc),
                                        endPeriod = DateTime.SpecifyKind(startPeriod, DateTimeKind.Utc), // bkPrdCliDto!.ddsou 
                                        TypeMag = typeMagResult.Id,
                                        CodeIN = "IN3",
                                        CodeDevise = apprint.DateValiditeAgenceCodeDeviseNumeroCompte.Substring(9, 3),
                                        EstActif = apprint.EstActifCodeTarifNumeroCompte!.Substring(0, 1),
                                        CodeAgence = apprint.DateValiditeAgenceCodeDeviseNumeroCompte.Substring(4, 5),
                                        TypeBeneficiaire = "AUTO",
                                        ReferenceBeneficiaire = 691228,
                                        CleBeneficiaire = 46,
                                        DatePrelevement = DateTime.SpecifyKind(startPeriod, DateTimeKind.Utc),
                                        PrixUnitCarte = prixMensuelCarte, // prix mensuel de la carte associée ou pas à un pack
                                        ReferenceOperation = "RVSA" + start.ToString("yy") + start.Month.ToString("D2") + start.Day.ToString("D2"),
                                        CodeOperation = "C",
                                        CodeEmetteur = "FACSER",
                                        IndicateurDomiciliation = "N",
                                        LibelleCarte = BuildLibelleCarte(apprint.EstActifCodeTarifNumeroCompte, apprint.CodeCarte, startPeriod),
                                        Carte = apprint.NumCarte!,
                                        Sequence = "001",

                                    });
                                    continue;
                                }
                                else
                                {

                                    // recuperation de la ligne bkprdcli correspondant au numéro de compte
                                    bkPrdCliDto = await _bkPrdCliRepository.GetByNcpAsync(numeroCompte);

                                    // CLIENT SANS PACKAGE
                                    if (bkPrdCliDto == null)
                                    {
                                        _logger.LogWarning($"Ligne {ligne} : Numéro de compte {numeroCompte} non trouvé dans la base de données bkprdcli.");

                                        // AJOUTER LE CARTE A LA LISTE DE CARTE SANS PACKAGE
                                    }
                                    else
                                    {
                                        // CLIENT AVEC PACKAGE

                                        // si la carte est active et que sa date de création est POSTERIEUR à la période de début de prélèvement
                                        if (dateCreationCarte <= startPeriod && dateCreationCarte < bkPrdCliDto.ddsou)
                                        {

                                            int? nbMois = NombreMois(startPeriod, bkPrdCliDto.ddsou);

                                            if (nbMois != 0)
                                            {
                                                string codeTarifComplet = BuildCodeTarifComplet(apprint.EstActifCodeTarifNumeroCompte, apprint.CodeCarte);

                                                bkmvtis.Add(new Bkmvti
                                                {
                                                    NumeroCompte = numeroCompte,
                                                    DateCreationCarte = dateCreationCarte.Value,
                                                    DateValiditeCarte = dateValiditeCarte,
                                                    CodeTarif = codeTarifComplet,
                                                    CodeCarte = apprint.CodeCarte!,
                                                    startPeriod = endPeriod,
                                                    endPeriod = bkPrdCliDto.ddsou,
                                                    TypeMag = typeMagResult.Id,
                                                    CodeIN = "IN3",
                                                    CodeDevise = apprint.DateValiditeAgenceCodeDeviseNumeroCompte.Substring(9, 3),
                                                    EstActif = apprint.EstActifCodeTarifNumeroCompte!.Substring(0, 1),
                                                    CodeAgence = apprint.DateValiditeAgenceCodeDeviseNumeroCompte.Substring(4, 5),
                                                    TypeBeneficiaire = "AUTO",
                                                    ReferenceBeneficiaire = 691228,
                                                    CleBeneficiaire = 46,
                                                    DatePrelevement = endPeriod,
                                                    PrixUnitCarte = 2504, // prix mensuel de la carte associée ou pas à un pack
                                                    ReferenceOperation = "RVSA" + start.ToString("yy") + start.Month + start.Day,
                                                    CodeOperation = "C",
                                                    CodeEmetteur = "FACSER",
                                                    IndicateurDomiciliation = "N",
                                                    LibelleCarte = BuildLibelleCarte(apprint.EstActifCodeTarifNumeroCompte, apprint.CodeCarte, endPeriod),
                                                    Carte = apprint.NumCarte!,
                                                    Sequence = "001",

                                                });
                                            }
                                        }

                                    }
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
                        await _bkmvtiRepository.SaveBkmvtiAsync(bkmvtis);

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
                    Message = "Manque à gagner enregistrer avec succès!!!",

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

        // Helper: construit la clé complète du tarif (préfixe + code carte)
        private string BuildCodeTarifComplet(string? estActifCodeTarifNumeroCompte, string? codeCarte)
        {
            if (string.IsNullOrEmpty(estActifCodeTarifNumeroCompte) || string.IsNullOrEmpty(codeCarte))
                return string.Empty;

            string prefix = estActifCodeTarifNumeroCompte.Length >= 3 ? estActifCodeTarifNumeroCompte.Substring(1, 2) : estActifCodeTarifNumeroCompte;
            return prefix + codeCarte;
        }

        // Helper: construit le libellé sécurisé en vérifiant le dictionnaire
        private string BuildLibelleCarte(string? estActifCodeTarifNumeroCompte, string? codeCarte, DateTime startPeriod)
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

        public Bkmvti ConvertApprintToBkmvti(Apprints apprints, BkPrdCliDto bkPrdCliDto)
        {

            return new Bkmvti
            {
                CodeAgence = apprints.DateValiditeAgenceCodeDeviseNumeroCompte!.Substring(4, 5),
                CodeDevise = apprints.DateValiditeAgenceCodeDeviseNumeroCompte.Substring(9, 3),
                NumeroCompte = apprints.DateValiditeAgenceCodeDeviseNumeroCompte.Substring(12),
                DateValiditeCarte = DateTime.ParseExact(apprints.DateValiditeAgenceCodeDeviseNumeroCompte.Substring(0, 4), "yyMM", CultureInfo.InvariantCulture),
                DateCreationCarte = DateTime.ParseExact(apprints.DateCreationCarte, "yyMMdd", CultureInfo.InvariantCulture),
                EstActif = apprints.EstActifCodeTarifNumeroCompte!.Substring(0, 1),
                CodeCarte = apprints.CodeCarte!,
                Sequence = "001",
                CodeIN = "IN3",
                TypeBeneficiaire = "AUTO",
                ReferenceBeneficiaire = 691228,
                CleBeneficiaire = 46,
                DatePrelevement = DateTime.ParseExact("2025-01-31", "yyyy-MM-dd", CultureInfo.InvariantCulture),
                PrixUnitCarte = 5000, // prix mensuel de la carte associée ou pas à un pack
                ReferenceOperation = "RVSA250",
                CodeOperation = "C",
                CodeEmetteur = "FACSER",
                IndicateurDomiciliation = "N",
                LibelleCarte = "Ext. Horizon jan. 2025/ Redev Horizon avr. 2025",
                Carte = apprints.NumCarte!,
                CodeTarif = apprints.EstActifCodeTarifNumeroCompte!.Substring(1, 2),

            };

        }

        public byte[] GenerateFile(List<Bkmvti> bkmvtis)
        {
            var lignes = bkmvtis.Select(d => BuildLine(d));

            var contenu = string.Join(Environment.NewLine, lignes);

            return Encoding.UTF8.GetBytes(contenu);
        }


        private string BuildLine(Bkmvti bkmvtis)
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
                    bkmvtis.PrixUnitCarte.ToString(),
                    "C", // CodeOperation
                    bkmvtis.LibelleCarte, // Exple : Ext. Horizon jan. 2025/ Redev Horizon avr. 2025
                    "N",  // indicateur de domicilisation
                    $"RVSA{bkmvtis.DatePrelevement.ToString("yyMMdd")}",

                    // compléter jusqu’à ton format exact
                    " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ",

                    bkmvtis.CodeAgence,
                    " ",
                    " ",
                    "001",
                    " ",
                    " ",
                    bkmvtis.CodeAgence+"C",
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

        // ... (les autres méthodes TxtToExcel, GetComptesActifs, etc. restent inchangées)
        // Note: dans TxtToExcel j'ai laissé les switch existants pour remplir worksheet.Cells[row,8]
        // et remplacé les accès à cartePrix["Code inconnu"] par un fallback numérique ou "#N/A".

        int? NombreMois(DateTime? dateDebut, DateTime? dateFin)
        {
            return (dateFin?.Year - dateDebut?.Year) * 12 + (dateFin?.Month - dateDebut?.Month);
        }
        public byte[] TxtToBkmvti(List<Apprints> apprints, DateTime DateDebut, DateTime DateFin)
        {
            throw new NotImplementedException();
        }

        public int CalculerIntersection(DateTime? debutA, DateTime? finA, DateTime? debutB, DateTime? finB)
        {
            if (!debutA.HasValue || !finA.HasValue || !debutB.HasValue || !finB.HasValue)
                return 0; // Pas de calcul si une date est manquante

            DateTime debutIntersection = debutA.Value > debutB.Value ? debutA.Value : debutB.Value;
            DateTime finIntersection = finA.Value < finB.Value ? finA.Value : finB.Value;

            if (debutIntersection > finIntersection)
                return 0;

            int diffMois = (finIntersection.Year - debutIntersection.Year) * 12
                         + (finIntersection.Month - debutIntersection.Month);

            // On compte le mois en cours si les jours se chevauchent
            if (finIntersection.Day >= debutIntersection.Day)
                diffMois++;

            return diffMois;
        }

        public Dictionary<string, ComptesActifsResponse> GetComptesActifs(ExcelWorksheet worksheetCompteActif)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, ComptesOuvertsResponse> GetComptesOuvertResponse(ExcelWorksheet worksheetCompteOuvert)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, DateDsouPackEchuResponse> GetDsouPackEchuResponse(ExcelWorksheet worksheetDsouPackEchu)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, HistCptDebiteRedevCarteResponse> GetHistCptDebiteRedevCarteResponse(ExcelWorksheet worksheetHistCptDebiteRedev)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, PackagesActifsResponse> GetPackagesActifsResponse(ExcelWorksheet worksheetPackActif)
        {
            throw new NotImplementedException();
        }

        public byte[] TxtToExcel(List<Apprints> apprints, DateTime DateDebut, DateTime DateFin)
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
    }
}