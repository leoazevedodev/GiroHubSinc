using HubSincronizacao.Apis.Fotos.Dtos;
using HubSincronizacao.Apis.Services;
using HubSincronizacao.SeedWork;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;

namespace HubSincronizacao.Apis.Giro
{
    public class GerarGiroLojas
    {
        AlphaBuyServices _alphaBuyServices;

        public GerarGiroLojas(AlphaBuyServices alphaBuyServices)
        {
            _alphaBuyServices = alphaBuyServices;
        }

        [Function("GerarGiroLojas")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req, ILogger log)
        {
            //string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            //FilterRequest data = JsonConvert.DeserializeObject<FilterRequest>(requestBody);

            //string connectionString = "Data Source=tcp:alphabeto.database.windows.net,1433;Initial Catalog=BDBI;User ID=alphabeto;Password=N13tzsche";

            //DataTable dataTable = new DataTable();

            //var result = await new BulkCopy(dataTable, (connectionString, "Fotos"))
            //        .Procedure("GerarGiroLojas", data.lojaid, data.cnpj);

            //var ids = result.Select(x => x.Id).ToList();

            //var teste = _alphaBuyServices.GetCompradoPecas(ids);

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var requests = JsonConvert.DeserializeObject<List<ProcedureRequest>>(requestBody);

            var tasks = new List<Task>();

            foreach (var request in requests)
            {
                tasks.Add(ExecuteProcedure(request));
            }

            await Task.WhenAll(tasks);

            return new OkObjectResult("All procedures executed successfully.");

        }

        public static async Task ExecuteProcedure(ProcedureRequest request)
        {
            string connectionString = "Data Source=tcp:alphabeto.database.windows.net,1433;Initial Catalog=BDBI;User ID=alphabeto;Password=N13tzsche";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();

                using (SqlCommand cmd = new SqlCommand("GerarGiroLojas", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@cnpj", request.Cnpj);
                    cmd.Parameters.AddWithValue("@lojaid", request.LojaId);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        [Function("ObterLojas")]
        public static async Task<IActionResult> ObterLojas([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req, ILogger log)
        {
            string connectionString = "Data Source=tcp:alphabeto.database.windows.net,1433;Initial Catalog=BDBI;User ID=alphabeto;Password=N13tzsche";

            var stores = new List<Store>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();

                string query = "SELECT cadastro_de_lojas_lojaid AS lojaid, cadastro_de_lojas_cnpj AS cnpj FROM cadastro_de_lojas WHERE cadastro_de_lojas_ativo = 'True'";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            stores.Add(new Store
                            {
                                LojaId = reader["lojaid"].ToString(),
                                Cnpj = reader["cnpj"].ToString()
                            });
                        }
                    }
                }
            }


            return new OkObjectResult(stores);
        }
    }

    public class Store
    {
        public string LojaId { get; set; }
        public string Cnpj { get; set; }
    }

    public class ProcedureRequest
    {
        public string LojaId { get; set; }
        public string Cnpj { get; set; }
    }
}
