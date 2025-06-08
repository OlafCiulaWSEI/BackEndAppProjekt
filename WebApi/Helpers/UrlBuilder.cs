using Microsoft.AspNetCore.Http;
using System.Web;
using System.Linq;
using System.Collections.Generic;

namespace WebApi.Helpers;

public class UrlBuilder
{
    private readonly HttpRequest _request;

    public UrlBuilder(HttpRequest request)
    {
        _request = request;
    }

    public string BuildUrl(int pageNumber, int pageSize)
    {
        var queryParams = _request.Query
            .Where(q => q.Key != "pageNumber" && q.Key != "pageSize")
            .Select(q => $"{q.Key}={Uri.EscapeDataString(q.Value.ToString() ?? string.Empty)}");

        var baseUrl = $"{_request.Scheme}://{_request.Host}{_request.PathBase}{_request.Path}";
        var queryString = string.Join("&", queryParams);
        
        if (!string.IsNullOrEmpty(queryString))
        {
            queryString = "&" + queryString;
        }

        return $"{baseUrl}?pageNumber={pageNumber}&pageSize={pageSize}{queryString}";
    }

    public string BuildUrl(int pageNumber, int pageSize, string additionalParam, string additionalValue)
    {
        var queryParams = _request.Query
            .Where(q => q.Key != "pageNumber" && q.Key != "pageSize" && q.Key != additionalParam)
            .Select(q => $"{q.Key}={Uri.EscapeDataString(q.Value.ToString() ?? string.Empty)}");

        var baseUrl = $"{_request.Scheme}://{_request.Host}{_request.PathBase}{_request.Path}";
        var queryString = string.Join("&", queryParams);
        
        if (!string.IsNullOrEmpty(queryString))
        {
            queryString = "&" + queryString;
        }

        return $"{baseUrl}?pageNumber={pageNumber}&pageSize={pageSize}&{additionalParam}={Uri.EscapeDataString(additionalValue)}{queryString}";
    }
} 