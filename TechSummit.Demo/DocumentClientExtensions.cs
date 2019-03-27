using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Documents.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace TechSummit.Demo
{
    public static class DocumentClientExtensions
    {
        public static Task<IEnumerable<T>> ToListAsync<T>(this IQueryable<T> query, HttpResponse httpResponse) => query.AsDocumentQuery().ToListAsync(httpResponse);

        public static async Task<IEnumerable<T>> ToListAsync<T>(this IDocumentQuery<T> queryable, HttpResponse httpResponse)
        {
            var stopwatch = Stopwatch.StartNew();
            var list = new List<T>();
            while (queryable.HasMoreResults)
            {
                var response = await queryable.ExecuteNextAsync<T>();
                list.AddRange(response);
            }
            stopwatch.Stop();
            httpResponse.Headers.Add("CosmosDb-Ms", stopwatch.ElapsedMilliseconds.ToString());

            return list;
        }
    }
}
