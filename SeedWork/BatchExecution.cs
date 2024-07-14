using HubSincronizacao.Apis.Giro.Dtos;
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

        public async Task ExecuteProcedureAsync(string lojaId, string cnpj, string uf)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var cnpjParameter = new SqlParameter("@cnpj", cnpj);
                    var lojaIdParameter = new SqlParameter("@lojaid", lojaId);
                    var ufParameter = new SqlParameter("@uf", uf);

                    string sql = "EXEC GerarGiroLojas @cnpj, @lojaid, @uf";

                    await _context.Database.ExecuteSqlRawAsync(sql, cnpjParameter, lojaIdParameter, ufParameter);
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine($"Erro ao executar a procedure para a loja {lojaId}: {ex.Message}");
                }
            }
        }

        public async Task<List<AtualizarGiroResultDto>> ExecuteProcedureObterAtualizacaoAsync(string lojaId, string cnpj, string data)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var lojaIdParameter = new SqlParameter("@lojaid", lojaId);
                    var cnpjParameter = new SqlParameter("@cnpj", cnpj);
                    var dataParameter = new SqlParameter("@dataInicio", data);

                    string sql = "EXEC ObterAtualizacaoGiroLojas @lojaid, @cnpj, @dataInicio";

                    var results = await _context.Set<AtualizarGiroResultDto>()
                        .FromSqlRaw(sql, lojaIdParameter, cnpjParameter , dataParameter)
                        .ToListAsync();

                    await transaction.CommitAsync();
                    return results;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine($"Erro ao executar a procedure para a loja {lojaId}: {ex.Message}");
                    return new List<AtualizarGiroResultDto>();
                }
            }
        }

        public async Task ExecuteProcedureAtualizarGiroAsync(string lojaId, string cnpj, string prodids, string uf)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var cnpjParameter = new SqlParameter("@cnpj", cnpj);
                    var lojaIdParameter = new SqlParameter("@lojaid", lojaId);
                    var prodidsParameter = new SqlParameter("@prodIds", prodids);
                    var ufParameter = new SqlParameter("@uf", uf);

                    string sql = "EXEC AtualizarGiroLojas @lojaid, @cnpj, @prodIds, @uf";

                    await _context.Database.ExecuteSqlRawAsync(sql, cnpjParameter, lojaIdParameter, prodidsParameter, ufParameter);
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
