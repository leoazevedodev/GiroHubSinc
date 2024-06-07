using Dapper;
using HubSincronizacao.Apis.Fotos.Dtos;
using HubSincronizacao.SeedWork;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubSincronizacao.Apis.Services
{
    public class AlphabiServices
    {
        public List<FotosAlphabiDto> Get(FilterRequest filter)
        {

            string sql = $@"SELECT * FROM Fotos
                         ";
            //WHERE Referencia IN (SELECT cadastro_de_produtos_referencia FROM cadastro_de_produtos WHERE cadastro_de_produtos_colecao = '{filter.Colecao}' AND cadastro_de_produtos_mapa = '{filter.Mapa}')

            string connectionString = "Data Source=tcp:alphabeto.database.windows.net,1433;Initial Catalog=BDBI;User ID=alphabeto;Password=N13tzsche";

            using (var connection = new SqlConnection(connectionString))
                return connection.Query<FotosAlphabiDto>(sql, commandTimeout: 0).ToList();

        }

        public async Task<BulkCopy> Insert(DataTable obj)
        {
            try
            {
                string connectionString = "Data Source=tcp:alphabeto.database.windows.net,1433;Initial Catalog=BDBI;User ID=alphabeto;Password=N13tzsche";

                return await new BulkCopy(obj, (connectionString, "Fotos"))
                    .CarregarDadosAsync();

            }
            catch (Exception ex)
            {
                throw new Exception($"Não foi possível insertar os dados. {ex.Message}");
            }
        }

    }
}
