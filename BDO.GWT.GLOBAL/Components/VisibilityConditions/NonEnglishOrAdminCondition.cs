using Kentico.Xperience.Admin.Base.Authentication;
using Kentico.Xperience.Admin.Base.Forms;
using Microsoft.AspNetCore.Http;
using System;
using System.Text.RegularExpressions;

[assembly: RegisterFormVisibilityCondition(
    identifier: NonEnglishOrAdminCondition.IDENTIFIER,
    conditionType: typeof(NonEnglishOrAdminCondition),
    name: "Non-English culture or Administrator")]

public class NonEnglishOrAdminCondition : VisibilityCondition
{
    internal const string IDENTIFIER = "BDO.NonEnglishOrAdminVisibilityCondition";

    private readonly IAuthenticatedUserAccessor userAccessor;
    private readonly IHttpContextAccessor httpContextAccessor;
    private const string ENGLISH_PREFIX = "en";
    private const string LANGUAGE_SEGMENT_PATTERN = @"/([a-zA-Z]{2}(?:-[a-zA-Z]{2})?)_\d+(?:/|$)";

    public NonEnglishOrAdminCondition(IAuthenticatedUserAccessor userAccessor, IHttpContextAccessor httpContextAccessor)
    {
        this.userAccessor = userAccessor;
        this.httpContextAccessor = httpContextAccessor;
    }

    public override bool Evaluate(IFormFieldValueProvider formFieldValueProvider)
    {
        var currentUser = userAccessor.Get().Result;
        bool isAdmin = currentUser.IsAdministrator();
        var httpContext = httpContextAccessor.HttpContext;

        bool isNonEnglish = !CurrentLanguageStartsWithEn();

        return isNonEnglish || isAdmin;
    }
    private bool CurrentLanguageStartsWithEn()
    {
        string? referer = httpContextAccessor.HttpContext?
            .Request.Headers.Referer.ToString();

        if (string.IsNullOrEmpty(referer))
        {
            return false;
        }

        var match = Regex.Match(referer, LANGUAGE_SEGMENT_PATTERN);

        if (!match.Success)
        {
            return false;
        }

        string languageCode = match.Groups[1].Value;

        return languageCode.StartsWith(ENGLISH_PREFIX, StringComparison.OrdinalIgnoreCase);
    }
}