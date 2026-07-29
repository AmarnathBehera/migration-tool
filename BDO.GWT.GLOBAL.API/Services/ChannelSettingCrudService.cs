using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Queries;
using CMS.Base;
using CMS.DataEngine;

namespace BDO.GWT.GLOBAL.API.Services;

public interface IChannelSettingCrudService
{
    Task<ChannelSettingOutput[]> GetChannelSettings(SettingsQuery settingsQuery, CancellationToken cancellationToken);

    Task<ChannelSettingOutput[]?> GetChannelSetting(string codeName, CancellationToken cancellationToken);

  //  Task<ChannelSettingOutput> SetChannelSetting(string codeName, ChannelSettingInput input, CancellationToken cancellationToken);

}

public class ChannelSettingCrudService : IChannelSettingCrudService
{
    private readonly IInfoProvider<WebChannelSnippetInfo> _webChannelSnippetInfoProvider;
    private readonly IInfoProvider<EmailInfo> _emailInfoProvider;
    private readonly IInfoProvider<EventsWebinarsInfo> _eventsWebinarsInfoProvider;
    private readonly IInfoProvider<GeneralSettingInfo> _generalSettingInfoProvider;
    private readonly IInfoProvider<SEOSettingsInfo> _seoSettingsInfoProvider;
    private readonly IInfoProvider<PrintorShareInfo> _printorShareInfoProvider;
    private readonly IInfoProvider<FilteringInfo> _filteringInfoProvider;
    private readonly IInfoProvider<OurPeopleInfo> _ourPeopleInfoProvider;

    public ChannelSettingCrudService(
        IInfoProvider<WebChannelSnippetInfo> webChannelSnippetInfoProvider,
        IInfoProvider<EmailInfo> emailInfoProvider,
        IInfoProvider<EventsWebinarsInfo> eventsWebinarsInfoProvider,
        IInfoProvider<GeneralSettingInfo> generalSettingInfoProvider,
        IInfoProvider<SEOSettingsInfo> seoSettingsInfoProvider,
        IInfoProvider<PrintorShareInfo> printorShareInfoProvider,
        IInfoProvider<FilteringInfo> filteringInfoProvider,
        IInfoProvider<OurPeopleInfo> ourPeopleInfoProvider)
    {
        _webChannelSnippetInfoProvider = webChannelSnippetInfoProvider;
        _emailInfoProvider = emailInfoProvider;
        _eventsWebinarsInfoProvider = eventsWebinarsInfoProvider;
        _generalSettingInfoProvider = generalSettingInfoProvider;
        _seoSettingsInfoProvider = seoSettingsInfoProvider;
        _printorShareInfoProvider = printorShareInfoProvider;
        _filteringInfoProvider = filteringInfoProvider;
        _ourPeopleInfoProvider = ourPeopleInfoProvider;
    }
    public async Task<ChannelSettingOutput[]> GetChannelSettings(SettingsQuery settingsQuery, CancellationToken cancellationToken)
    {
        if (settingsQuery.DisplayName is not null)
        {
            if (settingsQuery.DisplayName == "Include Custom Scripts")
            {
                var webChannelSnippets = _webChannelSnippetInfoProvider.Get().GetEnumerableTypedResult();
                return webChannelSnippets.Select(ToChannelSettingOutput).ToArray();
            }
            else if (settingsQuery.DisplayName == "Email")
            {
                var emails = _emailInfoProvider.Get().GetEnumerableTypedResult();
                return emails.Select(ToChannelSettingOutput).ToArray();
            }
            else if (settingsQuery.DisplayName == "Events & Webinar")
            {
                var eventsWebinars = _eventsWebinarsInfoProvider.Get().GetEnumerableTypedResult();
                return eventsWebinars.Select(ToChannelSettingOutput).ToArray();
            }
            else if (settingsQuery.DisplayName == "General Setting")
            {
                var generalSettings = _generalSettingInfoProvider.Get().GetEnumerableTypedResult();
                return generalSettings.Select(ToChannelSettingOutput).ToArray();
            }
            else if (settingsQuery.DisplayName == "SEO Settings")
            {
                var seoSettings = _seoSettingsInfoProvider.Get().GetEnumerableTypedResult();
                return seoSettings.Select(ToChannelSettingOutput).ToArray();
            }
            else if (settingsQuery.DisplayName == "Print or Share")
            {
                var printorShares = _printorShareInfoProvider.Get().GetEnumerableTypedResult();
                return printorShares.Select(ToChannelSettingOutput).ToArray();
            }
            else if (settingsQuery.DisplayName == "Filtering")
            {
                var filterings = _filteringInfoProvider.Get().GetEnumerableTypedResult();
                return filterings.Select(ToChannelSettingOutput).ToArray();
            }
            else if (settingsQuery.DisplayName == "Our People")
            {
                var ourPeople = _ourPeopleInfoProvider.Get().GetEnumerableTypedResult();
                return ourPeople.Select(ToChannelSettingOutput).ToArray();
            }
            else
            {
                return System.Array.Empty<ChannelSettingOutput>();
            }
        }

        var allWebChannelSnippets = _webChannelSnippetInfoProvider.Get().GetEnumerableTypedResult();
        var allEmails = _emailInfoProvider.Get().GetEnumerableTypedResult();
        var allEventsWebinars = _eventsWebinarsInfoProvider.Get().GetEnumerableTypedResult();
        var allGeneralSettings = _generalSettingInfoProvider.Get().GetEnumerableTypedResult();
        var allSeoSettings = _seoSettingsInfoProvider.Get().GetEnumerableTypedResult();
        var allPrintorShares = _printorShareInfoProvider.Get().GetEnumerableTypedResult();
        var allFilterings = _filteringInfoProvider.Get().GetEnumerableTypedResult();
        var allOurPeople = _ourPeopleInfoProvider.Get().GetEnumerableTypedResult();

        var outputs = allWebChannelSnippets.Select(ToChannelSettingOutput)
            .Concat(allEmails.Select(ToChannelSettingOutput))
            .Concat(allEventsWebinars.Select(ToChannelSettingOutput))
            .Concat(allGeneralSettings.Select(ToChannelSettingOutput))
            .Concat(allSeoSettings.Select(ToChannelSettingOutput))
            .Concat(allPrintorShares.Select(ToChannelSettingOutput))
            .Concat(allFilterings.Select(ToChannelSettingOutput))
            .Concat(allOurPeople.Select(ToChannelSettingOutput))
            .ToArray();

        return outputs;
    }
    public async Task<ChannelSettingOutput[]?> GetChannelSetting(string codeName, CancellationToken cancellationToken)
    {
        if (codeName == "BDO.WebChannelSnippet")
        {
            var webChannelSnippets = _webChannelSnippetInfoProvider.Get().GetEnumerableTypedResult();
            return webChannelSnippets.Select(ToChannelSettingOutput).ToArray();
        }
        else if (codeName == "BDO.Email")
        {
            var emails = _emailInfoProvider.Get().GetEnumerableTypedResult();
            return emails.Select(ToChannelSettingOutput).ToArray();
        }
        else if (codeName == "BDO.EventsWebinars")
        {
            var eventsWebinars = _eventsWebinarsInfoProvider.Get().GetEnumerableTypedResult();
            return eventsWebinars.Select(ToChannelSettingOutput).ToArray();
        }
        else if (codeName == "BDO.GeneralSetting")
        {
            var generalSettings = _generalSettingInfoProvider.Get().GetEnumerableTypedResult();
            return generalSettings.Select(ToChannelSettingOutput).ToArray();
        }
        else if (codeName == "BDO.SEOSettings")
        {
            var seoSettings = _seoSettingsInfoProvider.Get().GetEnumerableTypedResult();
            return seoSettings.Select(ToChannelSettingOutput).ToArray();
        }
        else if (codeName == "BDO.PrintorShare")
        {
            var printorShares = _printorShareInfoProvider.Get().GetEnumerableTypedResult();
            return printorShares.Select(ToChannelSettingOutput).ToArray();
        }
        else if (codeName == "BDO.Filtering")
        {
            var filterings = _filteringInfoProvider.Get().GetEnumerableTypedResult();
            return filterings.Select(ToChannelSettingOutput).ToArray();
        }
        else if (codeName == "BDO.OurPeople")
        {
            var ourPeople = _ourPeopleInfoProvider.Get().GetEnumerableTypedResult();
            return ourPeople.Select(ToChannelSettingOutput).ToArray();
        }

        return null;
    }
    private static ChannelSettingOutput ToChannelSettingOutput(WebChannelSnippetInfo setting) =>
        new() { CodeName = setting.ClassName, DisplayName = "Include Custom Scripts", InheritFromGlobalSettings = false, Type = "WebChannelSnippet", Value = ToFieldDictionary(setting) };

    private static ChannelSettingOutput ToChannelSettingOutput(EmailInfo setting) =>
        new() { CodeName = setting.ClassName, DisplayName = "Email", InheritFromGlobalSettings = false, Type = "Email", Value = ToFieldDictionary(setting) };

    private static ChannelSettingOutput ToChannelSettingOutput(EventsWebinarsInfo setting) =>
        new() { CodeName = setting.ClassName, DisplayName = "Events & Webinar", InheritFromGlobalSettings = false, Type = "EventsWebinars", Value = ToFieldDictionary(setting) };

    private static ChannelSettingOutput ToChannelSettingOutput(GeneralSettingInfo setting) =>
        new() { CodeName = setting.ClassName, DisplayName = "General Setting", InheritFromGlobalSettings = false, Type = "GeneralSetting", Value = ToFieldDictionary(setting) };

    private static ChannelSettingOutput ToChannelSettingOutput(SEOSettingsInfo setting) =>
        new() { CodeName = setting.ClassName, DisplayName = "SEO Settings", InheritFromGlobalSettings = false, Type = "SEOSettings", Value = ToFieldDictionary(setting) };

    private static ChannelSettingOutput ToChannelSettingOutput(PrintorShareInfo setting) =>
        new() { CodeName = setting.ClassName, DisplayName = "Print or Share", InheritFromGlobalSettings = false, Type = "PrintorShare", Value = ToFieldDictionary(setting) };

    private static ChannelSettingOutput ToChannelSettingOutput(FilteringInfo setting) =>
        new() { CodeName = setting.ClassName, DisplayName = "Filtering", InheritFromGlobalSettings = false, Type = "Filtering", Value = ToFieldDictionary(setting) };

    private static ChannelSettingOutput ToChannelSettingOutput(OurPeopleInfo setting) =>
        new() { CodeName = setting.ClassName, DisplayName = "Our People", InheritFromGlobalSettings = false, Type = "OurPeople", Value = ToFieldDictionary(setting) };

    private static Dictionary<string, object> ToFieldDictionary(IDataContainer container) =>
        container.ColumnNames.ToDictionary(columnName => columnName, columnName => container.GetValue(columnName));
}