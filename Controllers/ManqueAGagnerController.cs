using Microsoft.AspNetCore.Mvc;
using SYSGES_MAGs.Models;
using SYSGES_MAGs.Models.ModelsDto;
using SYSGES_MAGs.Services.IServices;

namespace SYSGES_MAGs.Controllers
{
    public class ManqueAGagnerController:Controller
    {
        private readonly ILogger<ManqueAGagnerController> _logger;
        private readonly IMagProcessingService _magProcessingService;
        private readonly IBkmvtiService _bkmvtiService;

        public ManqueAGagnerController(IMagProcessingService magProcessingService, IBkmvtiService bkmvtiService, ILogger<ManqueAGagnerController> logger)
        {
            _magProcessingService = magProcessingService;
            _bkmvtiService = bkmvtiService;
            _logger = logger;
        }
        public async Task<IActionResult> Index()
        {
            IEnumerable<TypeMag> typeMags = await _magProcessingService.GetAllTypeMagsAsync();
            return View(typeMags);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> ProcessMagFiles(
                IFormFile apprint,
                IFormFile openAccount,
                IFormFile activeAccount,
                IFormFile dateLastSouPackEchu,
                IFormFile activePackage,
                IFormFile accountHisDebiteByRedevCard,
                string typeMag,
                DateTime? startPeriod,
                DateTime? endPeriod)
        {
            // Vérification des paramètres obligatoires
            if (apprint == null || apprint.Length == 0 ||
                openAccount == null || openAccount.Length == 0 ||
                activeAccount == null || activeAccount.Length == 0 ||
                dateLastSouPackEchu == null || dateLastSouPackEchu.Length == 0 ||
                activePackage == null || activePackage.Length == 0 ||
                accountHisDebiteByRedevCard == null || accountHisDebiteByRedevCard.Length == 0 ||
                string.IsNullOrEmpty(typeMag) ||
                !startPeriod.HasValue || !endPeriod.HasValue)
            {
                _logger.LogInformation("Tous les champs sont obligatoires");
                return Json(new { success = false, message = "Tous les champs sont obligatoires" });
            }

            try
            {
                // Appel du service async correctement
                var result = await _magProcessingService.ProcessTxtExcelFiles(
                    apprint,
                    openAccount,
                    activeAccount,
                    dateLastSouPackEchu,
                    activePackage,
                    accountHisDebiteByRedevCard,
                    typeMag,
                    startPeriod.Value,
                    endPeriod.Value
                );

                if (!result.Success)
                {
                    _logger.LogWarning("Erreur lors du traitement des fichiers : " + result.Message);
                    return Json(new { success = false, message = result.Message });
                }

                _logger.LogInformation("Tous les fichiers ont été traités avec succès");

                return Json(new { success = true, message = $"Traitement réussi MAG : {typeMag}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur inattendue lors du traitement des fichiers MAG");
                return Json(new { success = false, message = "Une erreur est survenue lors du traitement des fichiers." });
            }
        }

        public async Task<IActionResult> Synthese(Guid id)
        {
            var result = await _magProcessingService.GetTypeMagWithSynthese(id);

            return View(result);
        }

        public async Task<IActionResult> DownloadBkmvti([FromBody] DownloadRequest request)
        {
            if (request.TypeMag == Guid.Empty) 
                return Json(new { success = true, message = "Identification du MAG invalide!!!" });

            //var bkmvtis = await _bkmvtiService.BkmvtisByMagType(request.TypeMag);
            var bkmvtis = await _bkmvtiService.BkmvtisByMagType(request.TypeMag);
            await _magProcessingService.IsDownloadAsync(request.TypeMag);
            if (bkmvtis == null || !bkmvtis.Any()) 
                return Json(new { success = true, message = "Impossible de télécharger le fichier" });

            var fileBytes = _magProcessingService.GenerateFile(bkmvtis);
            var fileName = $"BKMVTI_{DateTime.Now:yyyyMMddHHmmss}";
            return File(fileBytes, "application/octet-stream", fileName);
        }
    }
}
