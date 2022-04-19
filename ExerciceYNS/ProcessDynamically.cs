using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace ExerciceYNS
{
    public class ProcessDynamically
    {
        /// <summary>
        /// Méthode de traitement des messages au type non reconnu
        /// </summary>
        /// <param name="mySbMsg">Message émis par la souscription du Service Bus dédiée au contenu dynamique</param>
        [FunctionName("ProcessDynamically")]
        public static void Run(
            [ServiceBusTrigger("sbtexerciceyns", "sbsDynamic", Connection = "ServiceBusConnexionString")] string mySbMsg,
            [Table("Dynamic", Connection = "AzureWebJobsStorage")] CloudTable dynamicTable,
            ILogger log
            )
        {
            log.LogInformation($"Entering function ProcessDynamically");

            if (mySbMsg != null)
            {
                try
                {
                    //Ajout de la nouvelle entrée dans la table "Others"
                    Dictionary<string, string> data = JsonConvert.DeserializeObject<Dictionary<string, string>>(mySbMsg);
                    Dictionary<string, EntityProperty> processedData = new Dictionary<string, EntityProperty>();

                    foreach (var entityProperty in data)
                    {
                        processedData.Add(entityProperty.Key, new EntityProperty(entityProperty.Value.ToString()));
                    }

                    Guid rowKey = Guid.NewGuid();

                    DynamicTableEntity entity = new DynamicTableEntity("genericPk", rowKey.ToString(), "*", processedData);

                    TableOperation insert = TableOperation.Insert(entity);
                    dynamicTable.Execute(insert);

                    log.LogInformation($"New item has been successfully added to the table \"Dynamic\".");
                }
                //Gestion des exceptions liées au parsing json
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
                    log.LogInformation($"Leaving function ProcessDynamically");
                }
            }
        }
    }
}
