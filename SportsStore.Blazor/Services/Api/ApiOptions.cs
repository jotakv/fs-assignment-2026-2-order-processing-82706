namespace SportsStore.Blazor.Services.Api;

public sealed class ApiOptions
{
    public const string SectionName = "Api";

    public string BaseUrl { get; set; } = "https://localhost:7061/";
}
