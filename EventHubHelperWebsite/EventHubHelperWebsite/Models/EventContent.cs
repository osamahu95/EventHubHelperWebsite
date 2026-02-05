namespace EventHubHelperWebsite.Models
{
    public class EventContent
    {
        public long SequenceNumber { get; set; }
        public string Content { get; set; }
        public DateTime EnqueuedTime { get; set; }
    }
}
