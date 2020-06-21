
using Yodha.AzDevops.Api.Models;

namespace Yodha.AzDevops.Api.Responses
{
    /// <summary>
    /// Class
    /// <c>BuildDefinition</c> Response object for GetBuildDefinition
    /// </summary>
    public class BuildDefinitionResponse{
        public string Name { get; set; }
        public string Url { get; set; }
        public int Id { get; set; }
        public string Status { get; set; }
        public string Result { get; set; }
        public TriggerException Exception {get; set;}
    }

}
