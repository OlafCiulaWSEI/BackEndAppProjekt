using Microsoft.AspNetCore.Http;
using System.Web;

namespace WebApi.Helpers;

public class UrlBuilder
{
    private readonly HttpRequest _request;
    private readonly string? _legoSetId;

    public UrlBuilder(HttpRequest request, string? legoSetId = null)
    {
        _request = request;
        _legoSetId = legoSetId;
    }

    public string BuildUrl(int pageNumber, int pageSize)
    {
        var uriBuilder = new UriBuilder($"{_request.Scheme}://{_request.Host}{_request.Path}");
        var query = HttpUtility.ParseQueryString(string.Empty);
        
        query["pageNumber"] = pageNumber.ToString();
        query["pageSize"] = pageSize.ToString();
        
        if (_legoSetId != null)
        {
            query["legoSetId"] = _legoSetId;
        }

        // Zachowaj inne parametry z oryginalnego zapytania
        foreach (var key in _request.Query.Keys)
        {
            if (key != "pageNumber" && key != "pageSize" && key != "legoSetId")
            {
                query[key] = _request.Query[key];
            }
        }

        uriBuilder.Query = query.ToString();
        return uriBuilder.Uri.ToString();
    }
} 