using CreateCities.Application.Interfaces;
using CreateCities.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


var services = ConfigureServices();
var serviceProvider = services.BuildServiceProvider();
serviceProvider.GetService<ICreatorService>()?.Run().GetAwaiter().GetResult();

static IServiceCollection ConfigureServices()
{
    IServiceCollection services = new ServiceCollection();

    var config = LoadConfiguration();
    services.AddSingleton(config);
    services.AddTransient<ICreatorService, CreatorService>();
    return services;
}

static IConfiguration LoadConfiguration()
{
    var builder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);

    return builder.Build();
}