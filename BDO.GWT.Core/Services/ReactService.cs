
using BDO.GWT.Common.Utilities;
using BDO.GWT.Common.Exceptions;
using BDO.GWT.Common.Options;
using BDO.GWT.Web.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;
using BDO.GWT.Core.Constants;
using BDO.GWT.Core.DTOs;
using Microsoft.Extensions.Logging;

namespace BDO.GWT.Core.Services;

public interface IReactService
{
    Task<IHtmlContent> RenderComponentAsync<TProps>(int pageId, string componentType, string persistentUniqueIdentifier, TProps props, CancellationToken cancellationToken);

    Task<IHtmlContent> RenderComponentAsync<TProps>(int pageId, string componentType, string persistentUniqueIdentifier, TProps props, Action<ReactComponentOptions> configureOptions, CancellationToken cancellationToken);

    IHtmlContent RenderCombinedScript();
}

public class ReactService : IReactService
{
    private readonly HttpClient _httpClient;
    private readonly List<ComponentData> _renderedComponents = [];
    private readonly IOptions<ReactOptions> _reactOptions;
    //private readonly IPageContext _pageContext;
    private readonly IReactCachingService _reactCachingService;
    private readonly ILogger<ReactService> _logger;

    public ReactService(IHttpClientFactory httpClientFactory, IOptions<ReactOptions> reactOptions, IReactCachingService reactCachingService, ILogger<ReactService> logger)
    {
        _httpClient = httpClientFactory.CreateClient(HttpClientName.React);
        _reactOptions = reactOptions;
        //_pageContext = pageContext;
        _reactCachingService = reactCachingService;
        _logger = logger;
    }

    public async Task<IHtmlContent> RenderComponentAsync<TProps>(int pageId, string componentType, string persistentUniqueIdentifier, TProps props, CancellationToken cancellationToken)
    {
        return await RenderComponentAsync(pageId, componentType, persistentUniqueIdentifier, props, null, cancellationToken);
    }

    public async Task<IHtmlContent> RenderComponentAsync<TProps>(int pageId, string componentType, string persistentUniqueIdentifier, TProps props, Action<ReactComponentOptions>? configureOptions, CancellationToken cancellationToken)
    {
        var dto = new ReactComponentDto<TProps>
        {
            Props = props,
            Options = new ReactComponentOptions(),
        };

        configureOptions?.Invoke(dto.Options);

        var encodedDtoHash = HashingUtility.GetBase64EncodedHash(dto);

        var cacheKey = $"{pageId}|{persistentUniqueIdentifier}|{encodedDtoHash}";

        var body = await _reactCachingService.LoadAsync(async _ => await RenderComponentExternallyAsync(componentType, dto, cancellationToken), cacheKey, cancellationToken);

        var component = new ComponentData
        {
            ComponentType = componentType,
            ContainerId = body.ContainerId,
            Props = props,
        };

        _renderedComponents.Add(component);

        return new HtmlString(body.HtmlContent);
    }

    private async Task<ReactResponseBodyDto> RenderComponentExternallyAsync<TProps>(string componentType, ReactComponentDto<TProps> dto, CancellationToken cancellationToken)
    {
        var url = new Uri(_reactOptions.Value.ServerUrl, $"/api/render/{componentType}");

        var response = await _httpClient.PostAsJsonAsync(url, dto, cancellationToken);

        await HttpResponseUtility.EnsureResponseIsSuccessfulAsync(response, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(content))
        {
            _logger.LogError(
            "React renderer returned an empty response. Component: {ComponentType}, Url: {Url}, StatusCode: {StatusCode}, PropsType: {PropsType}",
            componentType,
            url,
            response.StatusCode,
            typeof(TProps).FullName);
            return new ReactResponseBodyDto
            {
                ContainerId = string.Empty,
                HtmlContent = string.Empty
            };
        }

        var body = await response.Content.ReadFromJsonAsync<ReactResponseBodyDto>(cancellationToken);

        if (body is null)
        {
            throw new UnexpectedBodyException($"Received empty response body when invoking '{url}' (status: {response.StatusCode}).");
        }

        return body;
    }

    public IHtmlContent RenderCombinedScript()
    {
        var htmlContent = new HtmlContentBuilder();

        htmlContent.AppendHtmlLine("<script>");

        foreach (var renderedComponent in _renderedComponents)
        {
            var json = JsonSerializer.Serialize(renderedComponent.Props, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            });

            htmlContent.AppendHtmlLine($"window.widgetRenderer.hydrate('{renderedComponent.ComponentType}', '{renderedComponent.ContainerId}', {json});");
        }

        htmlContent.AppendHtmlLine("</script>");

        return htmlContent;
    }

    private sealed record ComponentData
    {
        public required string ComponentType { get; init; }

        public required string ContainerId { get; init; }

        public required object? Props { get; init; }
    }
}


