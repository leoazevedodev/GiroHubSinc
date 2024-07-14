using HubSincronizacao.Apis.Giro.Dtos;
using HubSincronizacao.SeedWork;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace HubSincronizacao.Apis.Giro
{
    public class AtualizarGiroLojasOrquestrador
    {
        public readonly BatchExecution _batchExecution;

        public AtualizarGiroLojasOrquestrador(BatchExecution batchExecution)
        {
            _batchExecution = batchExecution;
        }

        [Function(nameof(AtualizarGiroLojasOrquestrador))]
        public static async Task RunOrchestrator([OrchestrationTrigger] TaskOrchestrationContext context)
        {
            var storeRequests = context.GetInput<RequestAtualizarGiroLojas>();

            var extractResult = await context.CallActivityAsync<List<AtualizarGiroResultDto>>(nameof(ExecuteStoredProcedureObterAtualizacao), storeRequests);

            var prodis = new List<string>();
            prodis = extractResult.Select(x => x.ProdId).ToList();
            
            storeRequests.ProdIds = FormatProdids.FormatProdIds(prodis);

            if (prodis.Count > 0)
            {
                await context.CallActivityAsync(nameof(ExecuteStoredProcedureAtualizarGiro), storeRequests);
            }
        }

        [Function(nameof(ExecuteStoredProcedureObterAtualizacao))]
        public async Task<List<AtualizarGiroResultDto>> ExecuteStoredProcedureObterAtualizacao([ActivityTrigger] RequestAtualizarGiroLojas param)
        {
            return await _batchExecution.ExecuteProcedureObterAtualizacaoAsync(param.LojaId, param.Cnpj, param.Data);
        }

        [Function(nameof(ExecuteStoredProcedureAtualizarGiro))]
        public async Task ExecuteStoredProcedureAtualizarGiro([ActivityTrigger] RequestAtualizarGiroLojas param)
        {
            await _batchExecution.ExecuteProcedureAtualizarGiroAsync(param.LojaId, param.Cnpj, param.ProdIds, param.Uf);
        }

        [Function("AtualizarGiroHttpStart")]
        public static async Task<HttpResponseData> HttpStart([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req, [DurableClient] DurableTaskClient client, FunctionContext executionContext)
        {
            ILogger logger = executionContext.GetLogger("AtualizarGiroHttpStart");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<RequestAtualizarGiroLojas>(requestBody);

            string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
                nameof(AtualizarGiroLojasOrquestrador), data);

            logger.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);

            return await client.CreateCheckStatusResponseAsync(req, instanceId);
        }
    }
    
    public class RequestAtualizarGiroLojas
    {
        public string LojaId { get; set; }
        public string Uf { get; set; }
        public string Cnpj { get; set; }
        public string Data { get; set; }
        public string ProdIds { get; set; }
    }
}
