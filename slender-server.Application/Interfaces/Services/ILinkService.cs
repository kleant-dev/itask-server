// slender-server.Application/Interfaces/Services/ILinkService.cs

using slender_server.Application.Models.Common;

namespace slender_server.Application.Interfaces.Services;

public interface ILinkService
{
    LinkDto CreateLink(string endpointName, string rel, string method, object? routeValues = null);
    List<LinkDto> CreateLinks(params (string EndpointName, string Rel, string Method, object? RouteValues)[] links);
    List<LinkDto> CreatePaginationLinks(string endpointName, int pageNumber, int pageSize, int totalPages, object? additionalRouteValues = null);
}