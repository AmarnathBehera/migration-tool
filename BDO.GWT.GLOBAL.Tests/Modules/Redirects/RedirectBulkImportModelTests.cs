using Kentico.Xperience.Admin.Base.FormAnnotations;
using XperienceCommunity.Redirects.UIPages;

namespace BDO.GWT.GLOBAL.Tests.Modules.Redirects;

public class RedirectBulkImportModelTests
{
    [Test]
    public void Model_IsPublicClass()
    {
        Assert.That(typeof(RedirectBulkImportModel).IsPublic, Is.True);
    }

    [Test]
    public void Model_DefaultValues_AreExpected()
    {
        var model = new RedirectBulkImportModel();

        Assert.Multiple(() =>
        {
            Assert.That(model.WebsiteChannelName, Is.EqualTo(string.Empty));
            Assert.That(model.Delimiter, Is.EqualTo("semicolon"));
            Assert.That(model.HasHeaderRow, Is.True);
            Assert.That(model.CsvData, Is.EqualTo(string.Empty));
        });
    }

    [Test]
    public void WebsiteChannelName_HasRequiredValidationRule_AndDropdownAttribute()
    {
        var property = typeof(RedirectBulkImportModel).GetProperty(nameof(RedirectBulkImportModel.WebsiteChannelName))!;

        Assert.That(property.GetCustomAttributes(typeof(RequiredValidationRuleAttribute), false), Is.Not.Empty);

        var dropDown = property.GetCustomAttributes(typeof(DropDownComponentAttribute), false)
            .Cast<DropDownComponentAttribute>()
            .Single();

        Assert.Multiple(() =>
        {
            Assert.That(dropDown.Label, Is.EqualTo("Site"));
            Assert.That(dropDown.Order, Is.EqualTo(1));
            Assert.That(dropDown.DataProviderType, Is.EqualTo(typeof(ChannelDropdownProvider)));
        });
    }

    [Test]
    public void Delimiter_HasRequiredValidationRule_AndDropdownAttribute()
    {
        var property = typeof(RedirectBulkImportModel).GetProperty(nameof(RedirectBulkImportModel.Delimiter))!;

        Assert.That(property.GetCustomAttributes(typeof(RequiredValidationRuleAttribute), false), Is.Not.Empty);

        var dropDown = property.GetCustomAttributes(typeof(DropDownComponentAttribute), false)
            .Cast<DropDownComponentAttribute>()
            .Single();

        Assert.Multiple(() =>
        {
            Assert.That(dropDown.Label, Is.EqualTo("CSV delimiter"));
            Assert.That(dropDown.Order, Is.EqualTo(2));
            Assert.That(dropDown.Options, Is.EqualTo("semicolon;Semicolon\r\ncomma;Comma"));
        });
    }

    [Test]
    public void HasHeaderRow_HasCheckBoxComponentAttribute()
    {
        var property = typeof(RedirectBulkImportModel).GetProperty(nameof(RedirectBulkImportModel.HasHeaderRow))!;

        var checkBox = property.GetCustomAttributes(typeof(CheckBoxComponentAttribute), false)
            .Cast<CheckBoxComponentAttribute>()
            .Single();

        Assert.Multiple(() =>
        {
            Assert.That(checkBox.Label, Is.EqualTo("First row is a header"));
            Assert.That(checkBox.Order, Is.EqualTo(3));
        });
    }

    [Test]
    public void CsvData_HasExpectedValidationAndComponentAttributes()
    {
        var property = typeof(RedirectBulkImportModel).GetProperty(nameof(RedirectBulkImportModel.CsvData))!;

        Assert.That(property.GetCustomAttributes(typeof(RequiredValidationRuleAttribute), false), Is.Not.Empty);

        var attr = property.GetCustomAttributes(typeof(CsvFileUploadComponentAttribute), false)
            .Cast<CsvFileUploadComponentAttribute>()
            .Single();

        Assert.Multiple(() =>
        {
            Assert.That(attr.Label, Is.EqualTo("CSV file"));
            Assert.That(attr.Order, Is.EqualTo(4));
            Assert.That(attr.ExplanationTextAsHtml, Is.True);
            Assert.That(attr.ExplanationText, Does.Contain("Upload a CSV file."));
        });
    }
}
