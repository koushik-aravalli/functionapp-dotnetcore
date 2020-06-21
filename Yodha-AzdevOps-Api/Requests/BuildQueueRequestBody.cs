namespace Yodha.AzDevops.Api.Requests
{
    /// <summary>
    /// Class
    /// <c>BuildQueueRequestBody</c> will be used to build request for posting BuildQueue
    /// </summary>
    public class BuildQueueRequestBody
    {
        public BuildDefinition definition { get; set; }
        public string sourceBranch { get; set; }
        public string parameters { get; set; }
        public string reason {get; set;}
    }

    public class BuildDefinition
    {
        public int id { get; set; }
    }
}
