using BDO.GWT.Core.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BDO.GWT.Core;

public static class ErrorMessage
{
    public const string PageIsCheckedOutByAnotherUser = "Cannot update page because it is checked out by another user.";

    public static string DocumentCultureNotAllowedOnSite(LanguageTag languageTag, string siteName, IEnumerable<LanguageTag> allowedCultures)
    {
        return $"Document culture '{languageTag}' is not allowed on site '{siteName}'. Allowed cultures: {string.Join(", ", allowedCultures.Select(x => $"'{x}'"))}.";
    }

    public static string IncorrectLanguageTagFormat(string languageTag)
    {
        return $"Incorrect language tag '{languageTag}' format.";
    }

    public static string PageAlreadyExistsInCulture(LanguageTag? languageTag)
    {
        return $"Page already exists in culture '{languageTag}'.";
    }

    public static string PageNodeContainsChildren(IEnumerable<Guid> pageChildrenGUIDs)
    {
        return $"Page node contains children, please delete them first. Children GUIDs: {string.Join(", ", pageChildrenGUIDs)}.";
    }

    public static string PageWithGuidNotFound(Guid pageGUID)
    {
        return $"Cannot find page with page GUID '{pageGUID}'.";
    }

    public static string PageWithUrlSlugAlreadyExists(string urlSlug, int nodeParentID)
    {
        return $"A page with URL slug '{urlSlug}' already exists under parent node ID '{nodeParentID}' for this language.";
    }

    public static string ParentPageTypeDoesNotAllowChildPageType(Type parentPageType, Type childPageType)
    {
        return $"Parent page type '{parentPageType.Name}' does not allow child page type '{childPageType.Name}'.";
    }

    public static string ParentWithNodeIdNotFound(int parentNodeID)
    {
        return $"Parent with node ID '{parentNodeID}' not found.";
    }
}
