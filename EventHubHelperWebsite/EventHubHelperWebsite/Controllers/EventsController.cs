using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using EventHubHelperWebsite.Dto;
using EventHubHelperWebsite.Extensions;
using EventHubHelperWebsite.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace EventHubHelperWebsite.Controllers
{
    public class EventsController(AppSettings appSettings) : Controller
    {
        private readonly AppSettings appSettings = appSettings;

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Send(EventRequestDto request)
        {
            try
            {
                await using var producer = new EventHubProducerClient(appSettings.EventHubConnection, appSettings.EventHubName);
                using EventDataBatch eventDataBatch = await producer.CreateBatchAsync();

                // Chcek if the Json in request is a valid json
                string jsonString = request.Payload.ValidateJson();

                if (!eventDataBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes(jsonString))))
                    return BadRequest(new { success = false, message = "Event is too large for the batch." });

                await producer.SendAsync(eventDataBatch);
                return Ok(new { success = true, message = "Event sent successfully." });
            }
            catch (Exception ex)
            {

                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
