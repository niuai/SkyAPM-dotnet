using Cmb.SkyApm.Agent.AspNetCore;
using Cmb.SkyApm.Tracing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SkyApm.AspNetCore.Diagnostics;
using SkyApm.Tracing;

[assembly: HostingStartup(typeof(CmbSkyApmHostingStartup))]

namespace Cmb.SkyApm.Agent.AspNetCore
{
    public class CmbSkyApmHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.AddSkyAPM(ext => ext.AddAspNetCoreHosting());

                services.RemoveAll<ICarrierPropagator>();
                services.AddSingleton<ICarrierPropagator, CmbCarrierPropagator>();
                services.RemoveAll<IUniqueIdGenerator>();
                services.AddSingleton<IUniqueIdGenerator, CmbUniqueIdGenerator>();
                services.RemoveAll<ISegmentContextFactory>();
                services.AddSingleton<ISegmentContextFactory, CmbSegmentContextFactory>();
            });
        }
    }
}
