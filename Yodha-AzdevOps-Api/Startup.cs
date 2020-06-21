using System;
using System.Net.Http.Headers;
using Yodha.AzDevops.Api.ConfigurationOptions;
using Yodha.AzDevops.Api.Interfaces;
using Yodha.AzDevops.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

[assembly: FunctionsStartup(typeof(Yodha.AzDevops.Api.Startup))]

namespace Yodha.AzDevops.Api
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            builder.Services.AddOptions<AzKeyVaultOptions>()
                                                    .Configure<IConfiguration>((settings, configuration) =>
                                                                            {
                                                                                configuration.GetSection("AzKeyVaultOptions").Bind(settings);
                                                                            });

            builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            builder.Services.AddMvcCore().AddNewtonsoftJson();

            var monitor = builder.Services.BuildServiceProvider().GetService<IOptionsMonitor<AzDevOpsOptions>>();

            string devopsPersonalAccessToken;

            if (!environment.Equals("local"))
            {
                devopsPersonalAccessToken = (builder.Services.BuildServiceProvider()
                                                            .GetService<IOptionsMonitor<AzKeyVaultOptions>>()).CurrentValue.PatSecret;
            }
            else
            {
                devopsPersonalAccessToken = monitor.CurrentValue.PersonalAccessToken;
            }

            builder.Services.AddHttpClient<IAzDevOpsPipelineService, AzDevOpsPipelineService>(c =>
            {
                c.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", devopsPersonalAccessToken))));
            });

        }
    }
}