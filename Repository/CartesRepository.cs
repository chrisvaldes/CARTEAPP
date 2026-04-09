using SYSGES_MAGs.Data;
using SYSGES_MAGs.Models;
using SYSGES_MAGs.Repository.IRepository;

namespace SYSGES_MAGs.Repository
{
    //public class CartesRepository
    //    :ICartesRepository
    //{
    //    public class CartesRepository : ICartesRepository
    //    {
    //        private readonly ApplicationDbContext bank;

    //        public CartesRepository(ApplicationDbContext context)
    //        {
    //            bank = context;
    //        }

    //        public Dictionary<string, ComptesActifsResponse> ComptesActifs()
    //        {
    //            throw new NotImplementedException();
    //        }

    //        public Dictionary<string, ComptesOuvertsResponse> ComptesOuverts()
    //        {
    //            return bank.bkcom
    //               .FromSqlRaw(@"
    //                SELECT ncp, cfe, ife
    //                FROM ba.bacom 
    //                WHERE (ncp LIKE '02%' OR ncp LIKE '31%') 
    //                  AND cfe = 'N' 
    //                  AND IFE = 'N'")
    //               .AsNoTracking()
    //               .ToDictionary(
    //                   c => c.ncp,
    //                   c => new ComptesOuvertsResponse { ncp = c.ncp, cfe = c.cfe }
    //               );
    //        }

    //        public Dictionary<string, DateDsouPackEchuResponse> DateDernSouscripPackEchu()
    //        {
    //            throw new NotImplementedException();
    //        }

    //        public Dictionary<string, HistCptDebiteRedevCarteResponse> HistCptDebiteRedevCarte()
    //        {
    //            throw new NotImplementedException();
    //        }

    //        public Dictionary<string, PackagesActifsResponse> PackagesActifs()
    //        {
    //            return bank.bkpack
    //            .FromSqlRaw(@"
    //            SELECT DISTINCT a.ncpf, a.cpack, b.lib
    //            FROM bank.bkprdcli a
    //            INNER JOIN bank.bkpack b ON a.cpack = b.cpack
    //            WHERE 
    //                (a.ncpf LIKE '02%' OR a.ncpf LIKE '31%')
    //                AND (a.dsou >= SYSDATE OR a.dfsou IS NULL)
    //                AND a.ctr <> '9'
    //                AND a.ncpf IN (
    //                    SELECT ncpf 
    //                    FROM bank.bkprdcli 
    //                    WHERE cpro NOT IN (
    //                        SELECT DISTINCT cpro 
    //                        FROM bank.bkprod
    //                    )
    //                )
    //        ")
    //            .AsNoTracking()
    //            .ToDictionary(
    //                   c => c.ncpf,
    //                   c => new PackagesActifsResponse { ncpf = c.ncpf, cpack = c.cpack, lib = c.lib }
    //               ); ;
    //        }
    //    }
    //}
}
