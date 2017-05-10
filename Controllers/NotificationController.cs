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
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Options;
using vbfreereporting;

namespace vbfreereporting.Controllers
{
    [Route("api/[controller]")]
    public class NotificationController : Controller
    {
        readonly string _storageConnectionString;

        private readonly ApplicationSettings _appSettings;

        NotificationController(IOptions<ApplicationSettings> appSettings)
        {
            _appSettings = appSettings.Value;

            var telemetry = new Microsoft.ApplicationInsights.TelemetryClient();
            telemetry.TrackTrace("Creating controller");
            
            _storageConnectionString = _appSettings.StorageConnectionString;
            telemetry.TrackTrace("Creating controller" + _storageConnectionString);
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
