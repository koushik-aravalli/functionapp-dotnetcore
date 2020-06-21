using System.Threading.Tasks;
using Yodha.AzDevops.Api.Requests;
using Yodha.AzDevops.Api.Responses;

namespace Yodha.AzDevops.Api.Interfaces
{
    public interface IAzDevOpsPipelineService
    {
        Task<BuildDefinitionResponse> GetBuildDefinition();
        Task<PipelineQueueResponse> PostQueueBuild(string rawjson, bool shouldDeploy);
        Task<BuildDefinitionResponse> GetBuildQueueStatus(string buildId);
    }

}
