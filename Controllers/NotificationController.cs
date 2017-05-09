using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.IO;

namespace vbfreereporting.Controllers
{
    [Route("api/[controller]")]
    public class NotificationController : Controller
    {
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

                }
            }
			return result;
		}
    }
}
