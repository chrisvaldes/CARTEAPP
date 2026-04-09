using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using SYSGES_MAGs.Models;
using SYSGES_MAGs.Models.ModelsDto;

namespace SYSGES_MAGs.Services.IServices
{
    public interface IMagProcessingService
    {
        Task<ServiceResult<string>> ProcessTxtExcelFiles(IFormFile apprintFile, IFormFile openAccountFile, IFormFile activeAccount, IFormFile dateLastSouPackEchuFile, IFormFile activePackageFile, IFormFile accountHisDebiteByRedevCardFile, string typeMag, DateTime startPeriod, DateTime endPeriod);
        Dictionary<string, ComptesActifsResponse>  GetComptesActifs(ExcelWorksheet worksheetCompteActif);
        Dictionary<string, ComptesOuvertsResponse> GetComptesOuvertResponse(ExcelWorksheet worksheetCompteOuvert);
        Dictionary<string, DateDsouPackEchuResponse> GetDsouPackEchuResponse(ExcelWorksheet worksheetDsouPackEchu);
        Dictionary<string, HistCptDebiteRedevCarteResponse> GetHistCptDebiteRedevCarteResponse(ExcelWorksheet worksheetHistCptDebiteRedev);
        Dictionary<string, PackagesActifsResponse> GetPackagesActifsResponse(ExcelWorksheet worksheetPackActif);
        byte[] TxtToExcel(List<Apprints> apprints, DateTime DateDebut, DateTime DateFin);
        byte[] TxtToBkmvti(List<Apprints> apprints, DateTime DateDebut, DateTime DateFin);
        byte[] GenerateFile(List<Bkmvti> bkmvtis);

        Task<IEnumerable<TypeMag>> GetAllTypeMagsAsync();
        public Task<TypeMagWithSyntheseDto> GetTypeMagWithSynthese(Guid typeMagId);
        public Task<bool> IsDownloadAsync(Guid typeMagId);

    }
}
