using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebApiContrib.Core.Results;

namespace TechSummit.Demo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args).
            ConfigureServices(s =>
            {
                s.AddMvcCore().AddJsonFormatters();
                s.AddRouting();
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
                        await res.HttpContext.Ok(new
                        {
                            Title = "Kolumbien. Ein Länderporträt",
                            Author = "Martin Specht"
                        });
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
