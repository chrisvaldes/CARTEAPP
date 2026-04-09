using SYSGES_MAGs.Models;

namespace SYSGES_MAGs.Repository.IRepository
{
    public interface ICartesRepository
    {
        public Dictionary<string, ComptesActifsResponse> ComptesActifs();
        public Dictionary<string, ComptesOuvertsResponse> ComptesOuverts();
        public Dictionary<string, DateDsouPackEchuResponse> DateDernSouscripPackEchu();
        public Dictionary<string, PackagesActifsResponse> PackagesActifs();
        public Dictionary<string, HistCptDebiteRedevCarteResponse> HistCptDebiteRedevCarte();
    }
}
