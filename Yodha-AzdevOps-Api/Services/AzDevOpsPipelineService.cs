using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using Yodha.AzDevops.Api.Requests;
using Yodha.AzDevops.Api.Responses;
using Yodha.AzDevops.Api.Interfaces;
using Microsoft.Extensions.Options;
using Yodha.AzDevops.Api.ConfigurationOptions;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Yodha.AzDevops.Api.Models;

namespace Yodha.AzDevops.Api.Services
{

    /// <summary>
    /// Class
    /// <c>AzDevOpsService</c> Entry point for the service
    /// </summary>
    public partial class AzDevOpsPipelineService : IAzDevOpsPipelineService
    {
        private readonly ILogger<AzDevOpsPipelineService> _log;
        private readonly IOptions<AzDevOpsOptions> _azDevOpsOptions;
        private readonly IOptions<AzKeyVaultOptions> _azKvOptions;
        private readonly HttpClient _client;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public AzDevOpsPipelineService(HttpClient httpClient,
                                       IOptions<AzDevOpsOptions> devopsOptions,
                                       IHttpContextAccessor httpContextAccessor,
                                       IOptions<AzKeyVaultOptions> azKvOptions,
                                       ILogger<AzDevOpsPipelineService> log)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _client = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _azDevOpsOptions = devopsOptions ?? throw new ArgumentNullException((nameof(devopsOptions)));
            _azKvOptions = azKvOptions ?? throw new ArgumentNullException((nameof(azKvOptions)));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentException((nameof(httpContextAccessor)));
        }

        public async Task<BuildDefinitionResponse> GetBuildDefinition()
        {
            var url = $"https://dev.azure.com/{_azDevOpsOptions.Value.Organization}/{_azDevOpsOptions.Value.Project}/_apis/build/definitions/{_azDevOpsOptions.Value.BuildDefinitionId}?api-version=5.1";

            try
            {
                using (HttpResponseMessage response = await _client.GetAsync(url))
                {
                    if(!response.IsSuccessStatusCode){
                        return new BuildDefinitionResponse
                        {                           
                            Exception =  new TriggerException {
                                StatusCode = response.StatusCode,
                                Message = $"Failed to get build pipleine id {_azDevOpsOptions.Value.BuildDefinitionId} within the organization {_azDevOpsOptions.Value.Organization}"
                            }
                        };
                    }

                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();

                    dynamic data = JsonConvert.DeserializeObject(responseBody);

                    var buldDef = new BuildDefinitionResponse
                    {
                        Name = data.name,
                        Id = data.id,
                        Url = data.url,
                        Status = data.queueStatus
                    };
                    return buldDef;
                }
            }
            catch (Exception ex)
            {
                var buldDef = new BuildDefinitionResponse
                    {
                        Exception = new Models.TriggerException{
                            Message = ex.Message
                        }
                    };
                return buldDef;
            }
        }

        public async Task<PipelineQueueResponse> PostQueueBuild(string rawjson, bool shouldDeploy)
        {
            var url = $"https://dev.azure.com/{_azDevOpsOptions.Value.Organization}/{_azDevOpsOptions.Value.Project}/_apis/build/builds?api-version=5.1";

            var requiredParameters = GetRequiredParameters(rawjson);

            string parameters_obj = JsonConvert.SerializeObject(new
            {
                az_func_req = $"'{rawjson}'",
                serviceconnection_name = requiredParameters.ServiceconnectionName,
                publish_folder_name = requiredParameters.PublishFolderName,
                dryRun = $"${shouldDeploy}",
                PAT = _azKvOptions.Value.PatSecret
            });

            var requestBody = new BuildQueueRequestBody
            {
                definition = new BuildDefinition { id = Convert.ToInt32(_azDevOpsOptions.Value.BuildDefinitionId) },
                sourceBranch = _azDevOpsOptions.Value.SourceBranch,
                reason = "triggered",
                parameters = parameters_obj
            };

            try
            {
                using (HttpResponseMessage response = await _client.PostAsJsonAsync(url, requestBody))
                {
                    if(!response.IsSuccessStatusCode){
                        return new PipelineQueueResponse
                        {
                            Exception =  new TriggerException{
                                StatusCode = response.StatusCode,
                                Message = $"Failed to queue release pipleine id {_azDevOpsOptions.Value.BuildDefinitionId}"
                            }
                        };
                    }

                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    dynamic data = JsonConvert.DeserializeObject(responseBody);
                    var queue = new PipelineQueueResponse
                    {
                        Id = data.id,
                        BuildNumber = data.buildNumber,
                        Url = data.url,
                        QueueTime = data.queueTime
                    };
                    return queue;
                }
            }
            catch (Exception ex)
            {
                var queue = new PipelineQueueResponse
                    {
                        Exception = new Models.TriggerException{
                            Message = ex.Message
                        }
                    };
                return queue;

            }
        }

        public async Task<BuildDefinitionResponse> GetBuildQueueStatus(string buildId)
        {
            var url = $"https://dev.azure.com/{_azDevOpsOptions.Value.Organization}/{_azDevOpsOptions.Value.Project}/_apis/build/builds/{buildId}?api-version=5.1";

            try
            {
                using (HttpResponseMessage response = await _client.GetAsync(url))
                {
                    if(!response.IsSuccessStatusCode){
                        return new BuildDefinitionResponse
                        {                           
                            Exception =  new TriggerException{
                                StatusCode = response.StatusCode,
                                Message = $"Failed to get build with build id {buildId} within the organization {_azDevOpsOptions.Value.Organization}"
                            }
                        };
                    }

                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();

                    dynamic data = JsonConvert.DeserializeObject(responseBody);

                    var buldDef = new BuildDefinitionResponse
                    {
                        Name = data.buildNumber,
                        Id = data.id,
                        Status = data.status,
                        Result = data.result
                    };
                    return buldDef;
                }
            }
            catch (Exception ex)
            {
                var buldDef = new BuildDefinitionResponse
                    {
                        Exception = new Models.TriggerException{
                            Message = ex.Message
                        }
                    };
                return buldDef;
            }
        }

        private SelfServicePipelineParameter GetRequiredParameters(string rawjson)
        {
            dynamic rq = JsonConvert.DeserializeObject(rawjson);
            var selfServiceParams = new SelfServicePipelineParameter();

            // Switch ServiceConnection between Engineering and Management
            if(rq.Environment.EnvironmentType == "Engineering"){
                selfServiceParams.ServiceconnectionName="devops-azure-engineering";
            }else{
                string subscriptionLongName = rq.Environment.SubscriptionName;
                var subscriptionShortName = subscriptionLongName.Split(" ")[^1];
                selfServiceParams.ServiceconnectionName=$"devops-azure-{subscriptionShortName}";
            }

            selfServiceParams.PublishFolderName = $"{rq.Environment.SubscriptionName}-{rq.Environment.ServiceLongName}-{DateTime.Now.ToString("yyyyMMddHHmmss")}";

            return selfServiceParams;
        }

    }

    internal class SelfServicePipelineParameter
    {
        public string ServiceconnectionName {get; set;}
        public string PublishFolderName {get; set;}
        public string PersonalAccessToken { get; set; }
    }
}
