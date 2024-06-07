using Dapper;
using HubSincronizacao.Apis.Fotos.Dtos;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubSincronizacao.Apis.Services
{
    public class AlphaBuyTwoServices
    {
        public List<FotosAlphabiDto> Get(FilterRequest filter)
        {

            string sql = $@"
                            SELECT
                                CONCAT('https://alphastorageshared.blob.core.windows.net/imagens-de-produto/', pi.StorageName) Url,
                                pi.Reference Referencia,
                                ISNULL(cpi.FriendlySize, '') Tamanhos,
                                ISNULL(cpi.Feature, '') Fabricacao,
                                ISNULL(cpi.FriendlyDescription, '') DescricaoPrincipal,
                                ISNULL(cpi.SecondDescription, '') DescricaoSecundaria,
                                pi.WebSiteCover Principal,
                                pi.ERPCover ErpPrincipal
                                FROM ProductImage pi
                                INNER JOIN ComplementaryProductInformation cpi ON pi.Reference = cpi.Reference
                                INNER JOIN(SELECT DISTINCT Reference FROM Product 
                                                               
) p
                                                               ON pi.Reference = p.Reference
                                ORDER BY Referencia
                           ";
            //WHERE Collection LIKE '{filter.Colecao}' AND Map LIKE '{filter.Mapa}'
            string connectionString = "Server=tcp:server-alphabuy.database.windows.net,1433;Initial Catalog=bd-alphabuy;Persist Security Info=False;User ID=alphabeto;Password=N13tzsche;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;";

            using (var connection = new SqlConnection(connectionString))
                return connection.Query<FotosAlphabiDto>(sql, commandTimeout: 0).ToList();


        }
    }
}
