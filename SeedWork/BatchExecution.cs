using HubSincronizacao.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubSincronizacao.SeedWork
{
    public class BatchExecution
    {
        private readonly DataContext _context;

        public BatchExecution(DataContext context)
        {
            _context = context;
        }

        public async Task ExecuteProcedureAsync(string lojaId, string cnpj)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var cnpjParameter = new SqlParameter("@cnpj", cnpj);
                    var lojaIdParameter = new SqlParameter("@lojaid", lojaId);

                    string sql = "EXEC GerarGiroLojas @cnpj, @lojaid";

                    await _context.Database.ExecuteSqlRawAsync(sql, cnpjParameter, lojaIdParameter);
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine($"Erro ao executar a procedure para a loja {lojaId}: {ex.Message}");
                }
            }
        }
    }
}
