using CMS.Base;
using CMS.Websites;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BDO.GWT.GLOBAL.Services;

namespace BDO.GWT.GLOBAL.Components.EventHandler;

public sealed partial class NonAsciiUrlSlugHandler : IAsyncEventHandler<BeforeCreateWebPageEvent>
{
    private static readonly AsyncLocal<bool> IsHandling = new();
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(5);

    private static partial class RegexPatterns
    {
        [GeneratedRegex(@"[\s_]+")]
        internal static partial Regex WhitespaceAndUnderscore();

        [GeneratedRegex(@"[^a-z0-9\-]")]
        internal static partial Regex NonAlphanumericExceptDash();

        [GeneratedRegex(@"-+")]
        internal static partial Regex MultipleDashes();
    }

    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<NonAsciiUrlSlugHandler> _logger;

    public NonAsciiUrlSlugHandler(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<NonAsciiUrlSlugHandler> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    public async Task HandleAsync(BeforeCreateWebPageEvent asyncEvent, CancellationToken cancellationToken)
    {
        if (IsHandling.Value)
        {
            _logger.LogWarning(
                "Non-ASCII URL slug update skipped because recursion was prevented for content type {ContentTypeName} in website channel {WebsiteChannelId}.",
                asyncEvent.Data.ContentTypeName,
                asyncEvent.Data.WebsiteChannelID);
            return;
        }

        if (string.IsNullOrWhiteSpace(asyncEvent.Data.DisplayName))
        {
            _logger.LogWarning(
                "Non-ASCII URL slug update skipped because the display name was empty for content type {ContentTypeName} in website channel {WebsiteChannelId}.",
                asyncEvent.Data.ContentTypeName,
                asyncEvent.Data.WebsiteChannelID);
            return;
        }

        try
        {
            IsHandling.Value = true;

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(DefaultTimeout);

            // Create a scope to resolve the scoped service
            using var scope = _serviceScopeFactory.CreateScope();
            var replacementService = scope.ServiceProvider.GetService(typeof(INonAsciiUrlReplacementService)) as INonAsciiUrlReplacementService;

            if (replacementService is null)
            {
                _logger.LogWarning(
                    "Non-ASCII URL slug generation skipped because the replacement service could not be resolved for content type {ContentTypeName} in website channel {WebsiteChannelId}.",
                    asyncEvent.Data.ContentTypeName,
                    asyncEvent.Data.WebsiteChannelID);
                return;
            }

            // This converts non-ASCII characters using our configured mappings BEFORE Kentico normalizes
            var replacedValue = await replacementService.ApplyReplacementsAsync(
                asyncEvent.Data.DisplayName,
                asyncEvent.Data.WebsiteChannelID,
                cts.Token).ConfigureAwait(false);

            // Generate URL slug from the replaced value using standard URL formatting
            var urlSlug = GenerateUrlSlug(replacedValue);

            // Set the UrlPathSlug to our custom value
            // This prevents Kentico from applying its default normalization
            asyncEvent.Data.UrlPathSlug = urlSlug;

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation(
                    "Custom URL slug set: {UrlSlug} for content type {ContentTypeName} in website channel {WebsiteChannelId}.",
                    urlSlug,
                    asyncEvent.Data.ContentTypeName,
                    asyncEvent.Data.WebsiteChannelID);
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            IsHandling.Value = false;
        }
    }

    /// <summary>
    /// Generates a URL-safe slug from text by:
    /// 1. Converting to lowercase
    /// 2. Replacing spaces with dashes
    /// 3. Removing non-alphanumeric characters (except dashes)
    /// 4. Cleaning up multiple consecutive dashes
    /// </summary>
    private static string GenerateUrlSlug(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        // Convert to lowercase
        var slug = input.ToLowerInvariant();

        // Replace spaces and underscores with dashes
        slug = RegexPatterns.WhitespaceAndUnderscore().Replace(slug, "-");

        // Remove any remaining non-alphanumeric characters except dashes
        slug = RegexPatterns.NonAlphanumericExceptDash().Replace(slug, string.Empty);

        // Replace multiple consecutive dashes with single dash
        slug = RegexPatterns.MultipleDashes().Replace(slug, "-");

        // Trim dashes from start and end
        slug = slug.Trim('-');

        return slug;
    }
}
