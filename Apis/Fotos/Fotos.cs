using Dapper;
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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HubSincronizacao.Apis.Fotos
{
    public class Fotos
    {
        private readonly ILogger<Fotos> _logger;
        AlphabiServices _alphabiServices;
        AlphaBuyTwoServices _alphaBuyTwoServices;

        public Fotos(ILogger<Fotos> logger, AlphaBuyTwoServices alphaBuyTwoServices, AlphabiServices alphabiServices)
        {
            _logger = logger;
            _alphaBuyTwoServices = alphaBuyTwoServices;
            _alphabiServices = alphabiServices;
        }

        [Function("fotos")]
        public async Task<IActionResult> Run ([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req, ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            FilterRequest data = JsonConvert.DeserializeObject<FilterRequest>(requestBody);

            var alphabi = _alphabiServices.Get(data);

            var alphabuyTwo = _alphaBuyTwoServices.Get(data);

            var noExists = alphabuyTwo.Where(x => !alphabi.Any(y => y.Referencia.Equals(x.Referencia))).ToList().ToDataTable();

            var result = await _alphabiServices.Insert(noExists);

            return new OkObjectResult($"");
        }
    }
}
