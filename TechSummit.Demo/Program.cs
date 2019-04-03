using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TechSummit.Demo
{
    public class Program
    {
        private static readonly string[] AllowedLocations = typeof(LocationNames).GetFields(BindingFlags.Static | BindingFlags.Public).Where(x => x.IsLiteral && x.FieldType == typeof(string)).Select(x => (string)x.GetRawConstantValue()).Where(x => x != null).ToArray();

        public static IConfiguration Configuration { get; set; }

        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args).
            UseApplicationInsights().
            ConfigureAppConfiguration((hostingContext, config) =>
            {
                Configuration = config.Build();
            }).
            ConfigureServices(s =>
            {
                s.AddMvc();

                #region CosmosDB
                var cosmosDbConfig = ConfigurationBinder.Get<CosmosDbConfig>(Configuration.GetSection("CosmosDb"));
                s.AddSingleton(cosmosDbConfig);

                var connectionPolicy = new ConnectionPolicy
                {
                    ConnectionMode = ConnectionMode.Direct,
                    ConnectionProtocol = Protocol.Tcp,
                    UseMultipleWriteLocations = true
                };
                if (cosmosDbConfig.PreferredLocations != null && cosmosDbConfig.PreferredLocations.Any() && cosmosDbConfig.PreferredLocations.All(l => AllowedLocations.Contains(l)))
                {
                    // write
                    connectionPolicy.SetCurrentLocation(cosmosDbConfig.PreferredLocations.First());

                    // read
                    foreach (var location in cosmosDbConfig.PreferredLocations)
                    {
                        connectionPolicy.PreferredLocations.Add(location);
                    }
                }

                var client = new DocumentClient(new Uri(cosmosDbConfig.Endpoint), cosmosDbConfig.Key, connectionPolicy);
                client.OpenAsync().GetAwaiter().GetResult();

                s.AddSingleton(client);
                #endregion
            }).
            Configure(app =>
            {
                app.UseHttpsRedirection();
                app.UseMvc();
            });
    }
}
