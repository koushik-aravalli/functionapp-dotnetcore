using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using Yodha.AzDevops.Api.Requests;
using Yodha.AzDevops.Api.Interfaces;

namespace Yodha.AzDevops.Api.Functions
{
    public class TriggerPipeline
    {
        private readonly IAzDevOpsPipelineService _azDevOpsPipelineService;
        private readonly ILogger<TriggerPipeline> _log;

        public TriggerPipeline(IAzDevOpsPipelineService azDevopsPipelineService,
                                      ILogger<TriggerPipeline> log)
        {
            _azDevOpsPipelineService = azDevopsPipelineService ?? throw new ArgumentNullException(nameof(azDevopsPipelineService));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        [FunctionName("TriggerPipeline")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req)
        {
            _log.LogInformation($"Azure Function running in environment: {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}");
            bool isValidateOnly = Convert.ToBoolean(req.Query["validate"]);

            string requestBody = Regex.Replace(await new StreamReader(req.Body).ReadToEndAsync(), @"\t|\n|\r", "");
            try
            {
                var incommingRequest = JsonConvert.DeserializeObject<HighPrivilageTrigger<TriggerPipelineRequestBody>>(requestBody);
                _log.LogInformation($"{incommingRequest.TriggerData}");

                // Search for Build Definition
                var buildDefinition = await _azDevOpsPipelineService.GetBuildDefinition();

                // Queue the build
                string triggerDataInfo = Regex.Replace(JsonConvert.SerializeObject(incommingRequest.TriggerData), @"\t|\n|\r", "");
                var buildQueue = await _azDevOpsPipelineService.PostQueueBuild(rawjson: triggerDataInfo, shouldDeploy: isValidateOnly);

                var rsp = JsonConvert.SerializeObject(new { BuildDefinition = buildDefinition, Queue = buildQueue });

                _log.LogDebug($"Queue instance information Build number: {buildQueue.BuildNumber} \n \t ==> Started at {buildQueue.QueueTime} \n \t Check Status at : {buildQueue.Url}");

                if (buildQueue.Exception == null)
                {
                    return new AcceptedResult($"/api/GetPipelineStatus/{buildQueue.Id}",null);
                }

                return new JsonResult(new { BuildDefinition = buildDefinition, BuildQueue = buildQueue });

            }
            catch (System.Exception)
            {
                return new NotFoundResult();
            }

        }
    }
}