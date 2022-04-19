using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.ServiceBus;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Azure.ServiceBus;
using System.Text;
using Microsoft.Azure.WebJobs;

namespace Middleway.Formation.Function
{
    public static class InitFunction
    {
        [FunctionName("InitFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            [ServiceBus("sbtstockitemyns", Connection = "ServiceBusCommon", EntityType = EntityType.Topic)] IAsyncCollector<Message> collector,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];
            string setting = req.Query["setting"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Message message = new Message(Encoding.UTF8.GetBytes(requestBody));
            await collector.AddAsync(message);

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            if (!string.IsNullOrEmpty(setting))
                responseMessage += $" And my setting value is : {Environment.GetEnvironmentVariable(setting)}";

            return new OkObjectResult(responseMessage);
        }
    }
}
