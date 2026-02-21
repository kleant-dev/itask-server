namespace slender_server.Application.Models.Common;

public sealed class LinkDto
{
    public required string Href { get; set; }
    public required string Rel { get; set; }
    public required string Method { get; set; }
}