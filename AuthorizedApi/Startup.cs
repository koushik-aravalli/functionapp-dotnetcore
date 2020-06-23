using System;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Com.Sample.Interfaces;
using Com.Sample.Services;

[assembly: FunctionsStartup(typeof(Com.Sample.Api.Startup))]

namespace Com.Sample.Api
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddTransient<IAuthorizationService, AuthorizationService>().AddHttpClient();
        }
    }
}