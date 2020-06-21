using Yodha.AzDevops.Api.Models;

namespace Yodha.AzDevops.Api.Responses
{
    /// <summary>
    /// Class
    /// <c>PipelineQueueResponse</c> Response object returned on sucessful queuing the pipeline
    /// </summary>
    public class PipelineQueueResponse{
        public string Id { get; set; }
        public string BuildNumber { get; set; }
        public string QueueTime { get; set; }
        public string Url { get; set; }
        public TriggerException Exception { get; set; }
    }
}
