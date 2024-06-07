using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HubSincronizacao.Apis.Giro.Dtos;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HubSincronizacao.SeedWork
{
    public class BulkCopy : IDisposable
    {
        public DataTable Source { get; private set; }
        public (string connectionString, string tableName) Target { get; set; }
        public List<string> Notifications { get; private set; }
        public bool Success => Notifications.Count == 0;
        public int Count => Source.Rows.Count;
        private SqlConnection sqlConnection;
        private SqlBulkCopy bulkCopy;
        private StringBuilder beforeCommands;
        private StringBuilder afterCommands;
        private Dictionary<string, string> indexes;

        public BulkCopy(DataTable source, (string connectionString, string tableName) target)
        {
            Source = source;
            Target = target;
            sqlConnection = new SqlConnection(target.connectionString);
            bulkCopy = new SqlBulkCopy(sqlConnection);
            beforeCommands = new StringBuilder();
            afterCommands = new StringBuilder();
            Notifications = new List<string>();
            indexes = new();
            if (Count == 0)
                Notifications.Add("Não há dados para importar");
            try
            {
                sqlConnection.Open();
            }
            catch (Exception e)
            {
                Notifications.Add("Não foi possível abrir a conexão com o destino:");
                Notifications.Add(e.Message);
            }
        }

        public BulkCopy WithIndex(string columnName, string type)
        {
            indexes.Add(columnName, type);
            return this;
        }

        public BulkCopy CreateTemporaryTable()
        {
            if (Count == 0)
                return this;

            QueryStringDropTableIfExists(beforeCommands);
            beforeCommands.Append($"create table temp_{Target.tableName} ( \n");
            QueryStringOfFields(beforeCommands);
            if (indexes.Count > 0)
                QueryStringAddIndexes(indexes, beforeCommands);

            return this;

        }

        private void QueryStringDropTableIfExists(StringBuilder query)
        {
            query.Append($"IF OBJECT_ID(N'dbo.temp_{Target.tableName}', N'U') IS NOT NULL  \n");
            query.Append($"drop table temp_{Target.tableName}; \n");
        }

        private void QueryStringOfFields(StringBuilder query)
        {
            int countFields = Source.Columns.Count;
            int indexOfLastField = countFields - 1;
            for (int i = 0; i < countFields; i++)
            {
                var column = Source.Columns[i];
                string field = column.ColumnName;
                string typeField = column.DataType.Name;
                bool isLast = i == indexOfLastField;
                query.Append($"{field} {ToSqlType(typeField)} {UseComma(!isLast)} \n");
                if (isLast) query.Append(");");
            }
        }

        private string UseComma(bool check) => (check ? "," : "");
        private string ToSqlType(string typeField)
        => typeField switch
        {
            "Int32" => "int",
            "Int64" => "int",
            "Boolean" => "bit",
            "Double" => "float",
            "Char" => "char(1)",
            _ => "nvarchar(max)"
        };
        private void QueryStringAddIndexes(Dictionary<string, string> indexes, StringBuilder query)
        {
            foreach (var index in indexes)
            {
                query.Append($"alter table temp_{Target.tableName} alter column {index.Key} {index.Value}");
                query.Append($"create index idx_temp_{Target.tableName}_{index.Key} on temp_{Target.tableName} ({index.Key});");
            }
        }


        public BulkCopy Merge(string procedureName, string integracao)
        {
            if (Count == 0)
                return this;
            afterCommands.Append($"exec {procedureName} @Integracao = {integracao};\n");
            return this;
        }

        public async Task<IActionResult> Procedure(string procedureName, string lojaid, string cnpj)
        {
            using (SqlConnection conn = new SqlConnection(Target.connectionString))
            {
                await conn.OpenAsync();

                using (SqlCommand cmd = new SqlCommand($"{procedureName}", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@cnpj", cnpj);
                    cmd.Parameters.AddWithValue("@lojaid", lojaid);

                    int rowsAffected = await cmd.ExecuteNonQueryAsync();

                    if (rowsAffected > 0)
                    {
                        return new OkObjectResult($"Procedure executed successfully, {rowsAffected} rows affected.");
                    }
                    else
                    {
                        return new OkObjectResult("Procedure executed successfully, no rows affected.");
                    }
                }
            }
        }



        public List<GiroLojasDto> ExecuteProcedure(string procedureName, SqlParameter[] parameters = null)
        {
            List<GiroLojasDto> results = new List<GiroLojasDto>();

            using (SqlConnection connection = new SqlConnection(Target.connectionString))
            {
                using (SqlCommand command = new SqlCommand(procedureName, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            GiroLojasDto item = new GiroLojasDto
                            {
                                Id = reader["Id"].ToString(),
                                LojaId = reader["LojaId"].ToString(),
                                Loja = reader["Loja"].ToString(),
                                ProdId = reader["ProdId"].ToString(),
                                Vendido = reader["Vendido"] as int?,
                                Comprado = reader["Comprado"] as int?,
                                Saldo = reader["Saldo"] as int?,
                                Giro = reader["Giro"].ToString(),
                                Descricao = reader["Descricao"].ToString(),
                                Tamanho = reader["Tamanho"].ToString(),
                                Cor = reader["Cor"].ToString(),
                                Colecao = reader["Colecao"].ToString(),
                                Mapa = reader["Mapa"].ToString(),
                                Referencia = reader["Referencia"].ToString(),
                                Grupo = reader["Grupo"].ToString(),
                                Categoria = reader["Categoria"].ToString(),
                                Coordenado = reader["Coordenado"].ToString(),
                                PrecoVenda = reader["PrecoVenda"] as double?,
                                PrecoCusto = reader["PrecoCusto"] as double?
                            };

                            results.Add(item);
                        }
                    }
                }
            }

            return results;
        }

        public BulkCopy DeleteTemporaryTable()
        {
            if (Count == 0)
                return this;

            QueryStringDropTableIfExists(afterCommands);
            return this;
        }

        public async Task<BulkCopy> BuildAsync()
        {
            if (Count == 0)
                return this;
            if (beforeCommands.Length != 0)
            {
                using var beforeCommand = new SqlCommand(beforeCommands.ToString(), sqlConnection);
                beforeCommand.CommandTimeout = 0;
                await beforeCommand.ExecuteScalarAsync();
            }

            await WriteToServerAsync();

            if (afterCommands.Length != 0)
            {
                using var afterCommand = new SqlCommand(afterCommands.ToString(), sqlConnection);
                afterCommand.CommandTimeout = 0;
                await afterCommand.ExecuteScalarAsync();
            }



            return this;
        }

        private async Task<BulkCopy> WriteToServerAsync()
        {
            string tempTableName = $"temp_{Target.tableName}";
            try
            {
                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(sqlConnection))
                {
                    bulkCopy.BulkCopyTimeout = 0;
                    bulkCopy.BatchSize = Source.Rows.Count;
                    bulkCopy.DestinationTableName = tempTableName;
                    await bulkCopy.WriteToServerAsync(Source);
                }
            }
            catch (Exception e)
            {
                Notifications.Add("Não foi possível enviar os dados:");
                Notifications.Add(e.Message);
            }
            return this;
        }

        public async Task<BulkCopy> CarregarDadosAsync()
        {
            try
            {
                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(sqlConnection))
                {
                    bulkCopy.BulkCopyTimeout = 0;
                    bulkCopy.BatchSize = Source.Rows.Count;
                    bulkCopy.DestinationTableName = Target.tableName;
                    await bulkCopy.WriteToServerAsync(Source);
                    Dispose();
                }
            }
            catch (Exception e)
            {
                Notifications.Add("Não foi possível enviar os dados:");
                Notifications.Add(e.Message);
            }
            return this;
        }


        public void Dispose()
        {
            bulkCopy.Close();
            sqlConnection.Close();
            sqlConnection.Dispose();
        }
    }
}
