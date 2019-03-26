using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebApiContrib.Core.Results;

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
            ConfigureAppConfiguration((hostingContext, config) =>
            {
                Configuration = config.Build();
            }).
            ConfigureServices(s =>
            {
                s.AddMvcCore().AddJsonFormatters();
                s.AddRouting();

                var cosmosDbConfig = ConfigurationBinder.Get<CosmosDbConfig>(Configuration.GetSection("CosmosDb"));
                s.AddSingleton(cosmosDbConfig);

                var client = new DocumentClient(new Uri(cosmosDbConfig.Endpoint), cosmosDbConfig.Key, new ConnectionPolicy());
                if (cosmosDbConfig.PreferredLocations != null && cosmosDbConfig.PreferredLocations.Any())
                {
                    foreach (var location in cosmosDbConfig.PreferredLocations)
                    {
                        if (AllowedLocations.Contains(location))
                        {
                            client.ConnectionPolicy.PreferredLocations.Add(location);
                        }
                    }
                }

                s.AddSingleton(client);
            }).
            Configure(app =>
            {
                app.UseHttpsRedirection();

                app.UseRouter(r =>
                {
                    r.MapGet("heartbeat", async (req, res, routeData) =>
                    {
                        await res.HttpContext.StatusCode(200);
                    });

                    r.MapGet("books", async (req, res, routeData) =>
                    {
                        var client = req.HttpContext.RequestServices.GetRequiredService<DocumentClient>();
                        var cosmosDbConfig = req.HttpContext.RequestServices.GetRequiredService<CosmosDbConfig>();

                        var collection = UriFactory.CreateDocumentCollectionUri(cosmosDbConfig.DbName, cosmosDbConfig.CollectionName);

                        var query = client.CreateDocumentQuery(collection).AsDocumentQuery();
                        var entities = new List<Book>();
                        while (query.HasMoreResults)
                        {
                            foreach (var entity in await query.ExecuteNextAsync<Book>())
                            {
                                entities.Add(entity);
                            }
                        }

                        await res.HttpContext.Ok(entities);
                    });
                });
            });
    }

    public class Book
    {
        public string Title { get; set; }
        public string Author { get; set; }
    }
}
