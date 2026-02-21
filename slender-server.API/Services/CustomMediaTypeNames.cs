namespace slender_server.API.Services;

public static class CustomMediaTypeNames
{
    public static class Application
    {
        public const string JsonV1 = "application/json;v=1";
        public const string JsonV2 = "application/json;v=2";
        public const string HateoasJson = "application/vnd.slender.hateoas+json";
        public const string HateoasJsonV1 = "application/vnd.slender.hateoas.1+json";
        public const string HateoasJsonV2 = "application/vnd.slender.hateoas.2+json";

        public const string HateoasSubType = "hateoas";
    }
}