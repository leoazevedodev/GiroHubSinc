using HubSincronizacao.Apis.Giro.Dtos;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubSincronizacao.Apis.Services
{
    public class AlphaBuyServices
    {
        public List<CompradoPecasDto> GetCompradoPecas(List<string> ids)
        {

            string connectionString = "Server=tcp:alphabeto.database.windows.net,1433;Initial Catalog=BDBU;Persist Security Info=False;User ID=alphabeto;Password=N13tzsche;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;";

            List<CompradoPecasDto> results = new List<CompradoPecasDto>();


            if (ids == null || !ids.Any())
            {
                return results;
            }

            int batchSize = 3000; // Número de IDs por lote
            int totalBatches = (int)Math.Ceiling((double)ids.Count / batchSize);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open(); // Abra a conexão apenas uma vez

                for (int batch = 0; batch < totalBatches; batch++)
                {
                    var idsBatch = ids.Skip(batch * batchSize).Take(batchSize);
                    string idsString = string.Join(",", idsBatch.Select(id => $"'{id}'"));

                    using (SqlCommand command = new SqlCommand("GetCompradoGiroLojas", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        DataTable tvp = new DataTable();
                        tvp.Columns.Add(new DataColumn("Id", typeof(string)));
                        foreach (string id in ids)
                        {
                            tvp.Rows.Add(id);
                        }

                        SqlParameter tvpParam = command.Parameters.AddWithValue("@Ids", tvp);
                        tvpParam.SqlDbType = SqlDbType.Structured;
                        tvpParam.TypeName = "dbo.IdListGiro";

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                CompradoPecasDto item = new CompradoPecasDto
                                {
                                    Id = reader["Id"].ToString(),
                                    Comprado = reader.IsDBNull(reader.GetOrdinal("Comprado")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("Comprado"))
                                };
                                results.Add(item);
                            }
                        }
                    }
                }
            }

            return results;
        }
    }
}
