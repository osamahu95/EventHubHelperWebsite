using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs.Consumer;
using EventHubHelperWebsite.Models;
using Microsoft.AspNetCore.Mvc;

namespace EventHubHelperWebsite.Controllers
{
    public class HomeController(ILogger<HomeController> logger, AppSettings appSettings) : Controller
    {
        private readonly ILogger<HomeController> logger = logger;

        private readonly string EventHubConnection = appSettings.EventHubConnection;
        private readonly string EventHubName = appSettings.EventHubName;
        private readonly string ConsumerGroup = appSettings.ConsumerGroup;

        public async Task<IActionResult> Index()
        {
            var messages = new List<EventContent>();

            try
            {
                await using var consumer = new EventHubConsumerClient(ConsumerGroup, EventHubConnection, EventHubName);

                var options = new ReadEventOptions
                {
                    MaximumWaitTime = TimeSpan.FromSeconds(5)
                };

                var eventsCollection = consumer.ReadEventsAsync(startReadingAtEarliestEvent: true, options);
                await foreach(var partitionEvent in eventsCollection)
                {
                    if (partitionEvent.Data != null)
                    {
                        string content = Encoding.UTF8.GetString(partitionEvent.Data.EventBody.ToArray());
                        long SequenceNumber = partitionEvent.Data.SequenceNumber;

                        messages.Add(new EventContent
                        {
                            SequenceNumber = SequenceNumber,
                            Content = content,
                            EnqueuedTime = partitionEvent.Data.EnqueuedTime.UtcDateTime
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while reading events from Event Hub.");
            }

            if (messages.Any()) messages = messages.OrderByDescending(m => m.SequenceNumber).ToList();
            return View(messages);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
