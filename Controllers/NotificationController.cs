using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.IO;
using Microsoft.WindowsAzure; // Namespace for CloudConfigurationManager
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Queue; // Namespace for Queue storage types


namespace vbfreereporting.Controllers
{
    [Route("api/[controller]")]
    public class NotificationController : Controller
    {
        readonly string _storageConnectionString;
        NotificationController()
        {
            _storageConnectionString = Environment.GetEnvironmentVariable("APPSETTINGS_storageaccount");
        }

        // POST: api/notification
		[HttpPost]
        public IActionResult Post(string digest)
        {
        	IActionResult result = NotFound();
            using (var bodyReader = new StreamReader(Request.Body))
            {
    			var hostedEmailAlertAsJson = bodyReader.ReadToEnd();
	    		if (hostedEmailAlertAsJson != null)
                {
                    var storageAccount = CloudStorageAccount.Parse(_storageConnectionString);
                    var queueClient = storageAccount.CreateCloudQueueClient();
                    var queueReference = queueClient.GetQueueReference("vbnotification-queue");
                    queueReference.CreateIfNotExistsAsync().ContinueWith((o) =>
                    {
                        CloudQueueMessage message = new CloudQueueMessage(hostedEmailAlertAsJson);
                        queueReference.AddMessageAsync(message).ContinueWith((ob)=>{
                            result = Ok();
                        });
                    });
                }
            }
			return result;
		}
    }
}
