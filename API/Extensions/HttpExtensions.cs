using System.Text.Json;
using API.Helpers;

namespace API.Extensions
{
    public static class HttpExtensions
    {
        public static void AddPaginationHeader(this HttpResponse response, PaginationHeader header)
        {
            var jsonOptions = new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
            };
            
            response.Headers.Add(
                key: "Pagination",
                value: JsonSerializer.Serialize(header, jsonOptions));
            
            response.Headers.Add(
                key: "Access-Control-Expose-Headers",
                value: "Pagination");
        }
    }
}