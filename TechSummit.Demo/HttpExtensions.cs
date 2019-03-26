﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters.Json.Internal;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using WebApiContrib.Core.Results;

namespace TechSummit.Demo
{
    public static class HttpExtensions
    {
        private static readonly JsonArrayPool<char> JsonArrayPool = new JsonArrayPool<char>(ArrayPool<char>.Shared);

        public static T ReadFromJson<T>(this HttpContext httpContext)
        {
            var serializer = JsonSerializer.CreateDefault();
            using (var streamReader = new HttpRequestStreamReader(httpContext.Request.Body, Encoding.UTF8))
            using (var jsonTextReader = new JsonTextReader(streamReader))
            {
                jsonTextReader.ArrayPool = JsonArrayPool;
                jsonTextReader.CloseInput = false;

                var obj = serializer.Deserialize<T>(jsonTextReader);

                var results = new List<ValidationResult>();
                if (Validator.TryValidateObject(obj, new ValidationContext(obj), results))
                {
                    return obj;
                }

                httpContext.Response.StatusCode = 400;
                httpContext.Response.WriteJson(results);

                return default(T);
            }
        }

        public static void WriteJson<T>(this HttpResponse response, T obj)
        {
            response.ContentType = "application/json";

            var serializer = JsonSerializer.CreateDefault();
            using (var writer = new HttpResponseStreamWriter(response.Body, Encoding.UTF8))
            {
                using (var jsonWriter = new JsonTextWriter(writer))
                {
                    jsonWriter.ArrayPool = JsonArrayPool;
                    jsonWriter.CloseOutput = false;
                    jsonWriter.AutoCompleteOnClose = false;

                    serializer.Serialize(jsonWriter, obj);
                }
            }
        }
    }
}
