using HubSincronizacao.SeedWork;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace HubSincronizacao.Apis.Giro
{
    public class GerarLojasOrquestrador
    {
        public readonly BatchExecution _batchExecution;

        public GerarLojasOrquestrador(BatchExecution batchExecution)
        {
            _batchExecution = batchExecution;
        }

        [Function(nameof(GerarLojasOrquestrador))]
        public static async Task RunOrchestrator([Microsoft.Azure.Functions.Worker.OrchestrationTrigger] TaskOrchestrationContext context)
        {
            var storeRequests = context.GetInput<Request>();

            var tasks = new List<Task>();

            //for (int i = 1; i <= 80; i++)
            //{
            //    tasks.Add(context.CallActivityAsync("ExecuteStoredProcedure", i));
            //}
            tasks.Add(context.CallActivityAsync("ExecuteStoredProcedure", storeRequests));

            await Task.WhenAll(tasks);
        }

        [FunctionName("ExecuteStoredProcedure")]
        public async Task ExecuteStoredProcedure([Microsoft.Azure.WebJobs.Extensions.DurableTask.ActivityTrigger] Request data, ILogger log)
        {
            await _batchExecution.ExecuteProcedureAsync(data.Cnpj, data.LojaId);
        }

        [FunctionName("HttpStartGiroLojas")]
        public static async Task<HttpResponseData> HttpStart(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req,
            [Microsoft.Azure.WebJobs.Extensions.DurableTask.DurableClient] DurableTaskClient client,
            FunctionContext executionContext)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<Request>(requestBody);

            string instanceId = await client.ScheduleNewOrchestrationInstanceAsync("Orchestrator", data);

            return await client.CreateCheckStatusResponseAsync(req, instanceId);
        }
    }

    public class Request
    {
        public string LojaId { get; set; }
        public string Cnpj { get; set; }
    }
}
