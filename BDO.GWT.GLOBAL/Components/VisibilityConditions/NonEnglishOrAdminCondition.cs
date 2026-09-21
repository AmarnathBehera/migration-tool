#nullable enable

using Kentico.Xperience.Admin.Base.Authentication;
using Kentico.Xperience.Admin.Base.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Text.RegularExpressions;

[assembly: RegisterFormVisibilityCondition(
    identifier: BDO.GWT.GLOBAL.Components.VisibilityConditions.NonEnglishOrAdminCondition.IDENTIFIER,
    conditionType: typeof(BDO.GWT.GLOBAL.Components.VisibilityConditions.NonEnglishOrAdminCondition),
    name: "Non-English culture or Administrator")]

namespace BDO.GWT.GLOBAL.Components.VisibilityConditions;

public partial class NonEnglishOrAdminCondition : VisibilityCondition
{
    internal const string IDENTIFIER = "BDO.NonEnglishOrAdminVisibilityCondition";

    private readonly IAuthenticatedUserAccessor userAccessor;
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly ILogger<NonEnglishOrAdminCondition> logger;
    private const string ENGLISH_PREFIX = "en";
    private const string LANGUAGE_SEGMENT_PATTERN = @"/([a-zA-Z]{2}(?:-[a-zA-Z]{2})?)_\d+(?:/|$)";

    public NonEnglishOrAdminCondition(
        IAuthenticatedUserAccessor userAccessor,
        IHttpContextAccessor httpContextAccessor,
        ILogger<NonEnglishOrAdminCondition> logger)
    {
        this.userAccessor = userAccessor;
        this.httpContextAccessor = httpContextAccessor;
        this.logger = logger;
    }

    public override bool Evaluate(IFormFieldValueProvider formFieldValueProvider)
    {
        try
        {
            var currentUser = userAccessor.Get().Result;
            bool isAdmin = currentUser.IsAdministrator();

            bool isNonEnglish = !CurrentLanguageStartsWithEn();

            return isNonEnglish || isAdmin;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to evaluate {Condition}.", nameof(NonEnglishOrAdminCondition));
            return true;
        }
    }
    private bool CurrentLanguageStartsWithEn()
    {
        string? referer = httpContextAccessor.HttpContext?
            .Request.Headers.Referer.ToString();

        if (string.IsNullOrEmpty(referer))
        {
            return false;
        }

        var match = LanguageSegmentRegex().Match(referer);

        if (!match.Success)
        {
            return false;
        }

        string languageCode = match.Groups[1].Value;

        return languageCode.StartsWith(ENGLISH_PREFIX, StringComparison.OrdinalIgnoreCase);
    }

    [GeneratedRegex(LANGUAGE_SEGMENT_PATTERN, RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex LanguageSegmentRegex();
}