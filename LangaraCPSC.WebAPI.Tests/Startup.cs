using LangaraCPSC.WebAPI.DbModels;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;
using Xunit.DependencyInjection;

namespace LangaraCPSC.WebAPI.Tests;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<LCSDBContext>();
    }
}
