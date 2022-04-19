using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ExerciceYNS
{
    public class ProcessInvoices
    {
        /// <summary>
        /// M�thode de traitement des messages de type facture (invoice)
        /// </summary>
        /// <param name="mySbMsg">Message �mis par la souscription du Service Bus d�di�e aux factures</param>
        [FunctionName("ProcessInvoices")]
        public async Task Run(
            [ServiceBusTrigger("sbtexerciceyns", "sbsFactures", Connection = "ServiceBusConnexionString")] string mySbMsg,
            [ServiceBus("sbqexerciceyns", Connection = "ServiceBusConnexionString")] IAsyncCollector<Message> collector,
            ILogger log
        )
        {
            log.LogInformation($"Entering function ProcessInvoices");

            if (mySbMsg != null)
            {
                try
                {
                    //Transformation du json en object de transport pour attester de son format
                    Invoice invoice = JsonConvert.DeserializeObject<Invoice>(mySbMsg);

                    log.LogInformation($"Processing invoice: id={invoice.Id}, customer={invoice.Customer}, amount={invoice.Amount}");

                    //Envoie de la requ�te et r�cup�ration de la r�ponse (Code 200 par d�faut)
                    HttpClient fakeApiClient = new HttpClient();
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://exercice-yns-factures.free.beeceptor.com/sendInvoice");
                    request.Content = JsonContent.Create(invoice);
                    HttpResponseMessage response = await fakeApiClient.SendAsync(request);

                    //Cr�ation du message � envoyer � la Queue du Service Bus
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    string responseStateMessage = response.IsSuccessStatusCode
                        ? $"Invoice n�{invoice.Id} successfully processed by the API :D."
                        : $"Error while processing invoice n�{invoice.Id} by the API :(.";

                    //Envoie du message � la Queue du Service Bus
                    Message message = new Message(Encoding.UTF8.GetBytes($"{responseStateMessage} Response message: {apiResponse}"));
                    await collector.AddAsync(message);

                    log.LogInformation($"Process result: {responseStateMessage}");
                    log.LogInformation($"API response: {apiResponse}");
                }
                //Gestion des exceptions li�es au parsing json
                catch(JsonSerializationException ex)
                {
                    log.LogError($"Deserialization error: {ex.Message}");
                }
                //Gestion des exceptions globales
                catch(Exception ex)
                {
                    log.LogError($"Global error processing message : {ex.Message}");
                }
                finally
                {
                    log.LogInformation($"Leaving function ProcessInvoices");
                }
            }
        }

        #region Classes
        /// <summary>
        /// Objet de transport pour une facture
        /// </summary>
        public class Invoice
        {
            public string Id { get; set; }
            public string Customer { get; set; }
            public string Amount { get; set; }
            public string Datatype { get; set; }
        }
        #endregion
    }
}
