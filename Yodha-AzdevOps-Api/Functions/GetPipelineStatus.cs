using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Yodha.AzDevops.Api.Interfaces;

namespace Yodha.AzDevops.Api.Functions
{
    public class GetPipelineStatus
    {
        private readonly IAzDevOpsPipelineService _azDevOpsPipelineService;
        private readonly ILogger<GetPipelineStatus> _log;

        public GetPipelineStatus(IAzDevOpsPipelineService azDevopsPipelineService,
                                      ILogger<GetPipelineStatus> log)
        {
            _azDevOpsPipelineService = azDevopsPipelineService ?? throw new ArgumentNullException(nameof(azDevopsPipelineService));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        [FunctionName("GetPipelineStatus")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "GetPipelineStatus/{id}")] HttpRequest req)
        {
            _log.LogInformation($"Azure Function running in environment: {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}");
            if (!req.Path.HasValue)
            {
                return new NotFoundResult();
            }

            string buildPipelineId = Convert.ToString(req.RouteValues["id"]);

            var buildStatusRsp = await _azDevOpsPipelineService.GetBuildQueueStatus(buildId: buildPipelineId);

            if (buildStatusRsp.Exception == null)
            {
                return new JsonResult(new { id=buildPipelineId, status=buildStatusRsp.Status, result=buildStatusRsp.Result });
            }

            return new NotFoundResult();
        }
    }
}