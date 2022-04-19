using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;

namespace ExerciceYNS
{
    public class ProcessOrders
    {
        /// <summary>
        /// M�thode de traitement des messages de type commande (order)
        /// </summary>
        /// <param name="mySbMsg">Message �mis par la souscription du Service Bus d�di�e aux commandes</param>
        [FunctionName("ProcessOrders")]
        public static void Run(
            [ServiceBusTrigger("sbtexerciceyns", "sbsCommandes", Connection = "ServiceBusConnexionString")]string mySbMsg,
            [Table("Orders", "order", Connection = "AzureWebJobsStorage")] CloudTable orders,
            ILogger log
            )
        {
            log.LogInformation($"Entering function ProcessOrders");

            if (mySbMsg != null)
            {
                try
                {
                    //Ajout de la nouvelle entr�e dans la table "Orders"
                    var order = JsonConvert.DeserializeObject<dynamic>(mySbMsg);
                    
                    order.PartitionKey = order.Datatype;
                    order.RowKey = order.Id;

                    TableOperation insert = TableOperation.Insert(order);
                    orders.Execute(insert);

                    log.LogInformation($"New Order has been successfully added to the database => {order}.");
                }
                //Gestion des exceptions li�es au parsing json
                catch (JsonSerializationException ex)
                {
                    log.LogError($"Deserialization error : {ex.Message}");
                }
                //Gestion des exceptions globales
                catch (Exception ex)
                {
                    log.LogError($"Global error processing message : {ex.Message}");
                }
                finally
                {
                    log.LogInformation($"Leaving function ProcessOrders");
                }
            }
        }

        #region Classes
        /// <summary>
        /// Repr�sentation de l'objet du mod�le pour une commande
        /// </summary>
        public class Order : TableEntity
        {
            public string Id { get; set; }
            public int Quantity { get; set; }
            public string Item { get; set; }
            public string Datatype { get; set; }
            public string Address { get; set; }
            public string City { get; set; }
            public string Zipcode { get; set; }
            public string Country { get; set; }
            public override string ToString()
            {
                return $"Id:{Id}, Quantity:{Quantity}, Item: {Item}, Datatype: {Datatype}, Address:{Address}, City:{City}, Zipcode:{Zipcode}, Country:{Country}";
            }
        }
        #endregion
    }
}
