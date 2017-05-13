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
using Microsoft.Extensions.Logging;

namespace vbfreereporting.Controllers
{
	[Route("api/[controller]")]
	public class NotificationController : Controller
	{
		readonly string _storageConnectionString;

		private readonly ApplicationSettings _appSettings;
		private readonly ILogger _logger;

		public NotificationController(IOptions<ApplicationSettings> appSettings, ILogger<NotificationController> logger)
		{
			_appSettings = appSettings.Value;
			//var telemetry = new Microsoft.ApplicationInsights.TelemetryClient();
			//telemetry.TrackTrace("Creating controller");
			//
			_storageConnectionString = _appSettings.StorageConnectionString;
			_logger = logger;
			_logger.LogInformation("Storage connection string: {0}", _storageConnectionString.Substring(0,10));

		}

		// POST: api/notification
		[HttpPost]
		public async Task<IActionResult> Post([FromQuery]string digest)
		{
			string hostedEmailAlertAsJson;
			using (var bodyReader = new StreamReader(Request.Body))
			{
				hostedEmailAlertAsJson = bodyReader.ReadToEnd();
			}
			_logger.LogTrace("Received {0}", hostedEmailAlertAsJson);
			if (_storageConnectionString.StartsWith("DefaultEndpointsProtocol"))
			{
				if (hostedEmailAlertAsJson != null)
				{
					var storageAccount = CloudStorageAccount.Parse(_storageConnectionString);
					var queueClient = storageAccount.CreateCloudQueueClient();
					var queueReference = queueClient.GetQueueReference("vbnotification-queue");
					await queueReference.CreateIfNotExistsAsync();

					CloudQueueMessage message = new CloudQueueMessage(hostedEmailAlertAsJson);
					await queueReference.AddMessageAsync(message);
					return Ok();
				}
			}
			return NotFound();
		}
	}
}
