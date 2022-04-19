using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ExerciceYNS
{
    public static class ConvertCSVToJson
    {
        [FunctionName("ConvertCSVToJson")]
        public static async Task<IActionResult> HttpStartAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Entering function ConvertCSVToJson");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            if (!string.IsNullOrEmpty(requestBody))
            {
                try
                {
                    string[] lines = requestBody.Replace("\r", "").Split('\n');

                    //Gestion de ces maudits retours à la ligne
                    lines = lines.Where(l => l.Length > 0).ToArray();

                    List<string[]> csv = new List<string[]>();

                    foreach (string line in lines)
                            csv.Add(line.Split(','));

                    string[] properties = lines[0].Split(',');

                    List<Dictionary<string, string>> listObjResult = new List<Dictionary<string, string>>();

                    for (int i = 1; i < lines.Length; i++)
                    {
                        Dictionary<string, string> objResult = new Dictionary<string, string>();
                        for (int j = 0; j < properties.Length; j++)
                            objResult.Add(properties[j], csv[i][j]);

                        listObjResult.Add(objResult);
                    }

                    log.LogInformation("Leaving function ConvertCSVToJson");

                    return new OkObjectResult(JsonConvert.SerializeObject(listObjResult));
                }
                catch (Exception ex)
                {
                    log.LogError($"Global error processing message : {ex.Message}");
                    return new BadRequestObjectResult(ex);
                }
                finally
                {
                    log.LogInformation($"Leaving function ProcessOrders");
                }
            } else
            {
                log.LogError($"No content found in request body");
                return null;
            }
        }
    }
}