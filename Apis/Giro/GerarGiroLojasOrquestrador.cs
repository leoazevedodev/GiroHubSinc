using HubSincronizacao.SeedWork;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace HubSincronizacao.Apis.Giro
{
    public class GerarGiroLojasOrquestrador
    {
        public readonly BatchExecution _batchExecution;

        public GerarGiroLojasOrquestrador(BatchExecution batchExecution)
        {
            _batchExecution = batchExecution;
        }

        [Function(nameof(GerarGiroLojasOrquestrador))]
        public static async Task RunOrchestrator([OrchestrationTrigger] TaskOrchestrationContext context)
        {
            var storeRequests = context.GetInput<Request>();

            await context.CallActivityAsync(nameof(ExecuteStoredProcedure), storeRequests);

        }

        [Function(nameof(ExecuteStoredProcedure))]

        public async Task ExecuteStoredProcedure([ActivityTrigger] Request param)
        {
            await _batchExecution.ExecuteProcedureAsync(param.Cnpj, param.LojaId, param.Uf);
        }

        [Function("GerarGiroHttpStart")]
        public static async Task<HttpResponseData> HttpStart([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req, [DurableClient] DurableTaskClient client, FunctionContext executionContext)
        {
            ILogger logger = executionContext.GetLogger("GerarGiroHttpStart");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<Request>(requestBody);

            string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
                nameof(GerarGiroLojasOrquestrador), data);

            logger.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);

            return await client.CreateCheckStatusResponseAsync(req, instanceId);
        }
    }
    public class Request
    {
        public string LojaId { get; set; }
        public string Cnpj { get; set; }
        public string Uf { get; set; }
    }
}
