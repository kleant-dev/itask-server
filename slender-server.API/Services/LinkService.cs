// slender-server.API/Services/LinkService.cs
using Microsoft.AspNetCore.Routing;
using slender_server.Application.Interfaces.Services;
using slender_server.Application.Models.Common;

namespace slender_server.API.Services;

public sealed class LinkService(
    LinkGenerator linkGenerator,
    IHttpContextAccessor httpContextAccessor)
    : ILinkService
{
    public LinkDto CreateLink(
        string endpointName,
        string rel,
        string method,
        object? routeValues = null)
    {
        var httpContext = httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("HttpContext is not available");

        string? href = linkGenerator.GetUriByName(
            httpContext,
            endpointName,
            routeValues);

        if (href is null)
        {
            throw new InvalidOperationException(
                $"Could not generate link for endpoint '{endpointName}'");
        }

        return new LinkDto
        {
            Href = href,
            Rel = rel,
            Method = method
        };
    }

    public List<LinkDto> CreateLinks(
        params (string EndpointName, string Rel, string Method, object? RouteValues)[] links)
    {
        return links
            .Select(l => CreateLink(l.EndpointName, l.Rel, l.Method, l.RouteValues))
            .ToList();
    }

    public List<LinkDto> CreatePaginationLinks(
        string endpointName,
        int pageNumber,
        int pageSize,
        int totalPages,
        object? additionalRouteValues = null)
    {
        var links = new List<LinkDto>();
        var routeValues = MergeRouteValues(additionalRouteValues, new { pageNumber, pageSize });

        links.Add(CreateLink(endpointName, "self", "GET", routeValues));

        if (pageNumber > 1)
        {
            var firstValues = MergeRouteValues(additionalRouteValues, new { pageNumber = 1, pageSize });
            links.Add(CreateLink(endpointName, "first", "GET", firstValues));
            
            var prevValues = MergeRouteValues(additionalRouteValues, new { pageNumber = pageNumber - 1, pageSize });
            links.Add(CreateLink(endpointName, "previous", "GET", prevValues));
        }

        if (pageNumber < totalPages)
        {
            var nextValues = MergeRouteValues(additionalRouteValues, new { pageNumber = pageNumber + 1, pageSize });
            links.Add(CreateLink(endpointName, "next", "GET", nextValues));
            
            var lastValues = MergeRouteValues(additionalRouteValues, new { pageNumber = totalPages, pageSize });
            links.Add(CreateLink(endpointName, "last", "GET", lastValues));
        }

        return links;
    }

    private static Dictionary<string, object?> MergeRouteValues(object? baseValues, object additionalValues)
    {
        var merged = new Dictionary<string, object?>();

        if (baseValues is not null)
        {
            foreach (var prop in baseValues.GetType().GetProperties())
            {
                merged[prop.Name] = prop.GetValue(baseValues);
            }
        }

        foreach (var prop in additionalValues.GetType().GetProperties())
        {
            merged[prop.Name] = prop.GetValue(additionalValues);
        }

        return merged;
    }
}