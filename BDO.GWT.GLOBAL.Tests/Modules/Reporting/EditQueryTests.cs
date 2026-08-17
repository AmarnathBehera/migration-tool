using BDO.GWT.CustomReportingModule;
using System.Reflection;
using System.Text.RegularExpressions;

namespace BDO.GWT.GLOBAL.Tests.Modules.Reporting;

[TestFixture]
public class EditQueryTests
{
    private Type _type = null!;

    [OneTimeSetUp]
    public void Setup()
    {
        _type = typeof(ReportingApplicationPage)
            .Assembly
            .GetTypes()
            .First(x => x.Name == "EditQuery");
    }

    #region GenerateQueryCodeName

    [Test]
    public void GenerateQueryCodeName_Replaces_Invalid_Characters()
    {
        var method = _type.GetMethod(
            "GenerateQueryCodeName",
            BindingFlags.NonPublic | BindingFlags.Static)!;

        var result = (string)method.Invoke(
            null,
            new object[] { "My Query!@#" })!;

        Assert.That(result, Does.StartWith("My_Query"));
    }

    [Test]
    public void GenerateQueryCodeName_Uses_Query_For_Empty_String()
    {
        var method = _type.GetMethod(
            "GenerateQueryCodeName",
            BindingFlags.NonPublic | BindingFlags.Static)!;

        var result = (string)method.Invoke(
            null,
            new object[] { "" })!;

        Assert.That(result, Does.StartWith("Query_"));
    }

    [Test]
    public void GenerateQueryCodeName_Trims_Periods()
    {
        var method = _type.GetMethod(
            "GenerateQueryCodeName",
            BindingFlags.NonPublic | BindingFlags.Static)!;

        var result = (string)method.Invoke(
            null,
            new object[] { "...Report..." })!;

        Assert.That(result, Does.StartWith("Report"));
    }

    [Test]
    public void GenerateQueryCodeName_Appends_Guid()
    {
        var method = _type.GetMethod(
            "GenerateQueryCodeName",
            BindingFlags.NonPublic | BindingFlags.Static)!;

        var result = (string)method.Invoke(
            null,
            new object[] { "Report" })!;

        Assert.That(
            Regex.IsMatch(
                result,
                @"^Report_[a-fA-F0-9]{32}$"),
            Is.True);
    }

    [Test]
    public void GenerateQueryCodeName_Returns_Unique_Value()
    {
        var method = _type.GetMethod(
            "GenerateQueryCodeName",
            BindingFlags.NonPublic | BindingFlags.Static)!;

        var first = (string)method.Invoke(
            null,
            new object[] { "Report" })!;

        var second = (string)method.Invoke(
            null,
            new object[] { "Report" })!;

        Assert.That(first, Is.Not.EqualTo(second));
    }

    #endregion

    #region SkipWhitespace

    [Test]
    public void SkipWhitespace_Moves_Index()
    {
        var method = _type.GetMethod(
            "SkipWhitespace",
            BindingFlags.NonPublic | BindingFlags.Static)!;

        object[] args =
        {
            "    SELECT",
            0
        };

        method.Invoke(null, args);

        Assert.That((int)args[1], Is.EqualTo(4));
    }

    [Test]
    public void SkipWhitespace_Does_Not_Move_When_Not_Whitespace()
    {
        var method = _type.GetMethod(
            "SkipWhitespace",
            BindingFlags.NonPublic | BindingFlags.Static)!;

        object[] args =
        {
            "SELECT",
            0
        };

        method.Invoke(null, args);

        Assert.That((int)args[1], Is.EqualTo(0));
    }

    #endregion

    #region SkipLineComment

    [Test]
    public void SkipLineComment_Returns_True_For_Comment()
    {
        var method = _type.GetMethod(
            "SkipLineComment",
            BindingFlags.NonPublic | BindingFlags.Static)!;

        object[] args =
        {
            "-- comment\nSELECT",
            0
        };

        var result = (bool)method.Invoke(
            null,
            args)!;

        Assert.That(result, Is.True);
    }

    [Test]
    public void SkipLineComment_Returns_False_For_Normal_Text()
    {
        var method = _type.GetMethod(
            "SkipLineComment",
            BindingFlags.NonPublic | BindingFlags.Static)!;

        object[] args =
        {
            "SELECT *",
            0
        };

        var result = (bool)method.Invoke(
            null,
            args)!;

        Assert.That(result, Is.False);
    }

    #endregion

    #region SkipBlockComment

    [Test]
    public void SkipBlockComment_Returns_True()
    {
        var method = _type.GetMethod(
            "SkipBlockComment",
            BindingFlags.NonPublic | BindingFlags.Static)!;

        object[] args =
        {
            "/* comment */ SELECT",
            0
        };

        var result = (bool)method.Invoke(
            null,
            args)!;

        Assert.That(result, Is.True);
    }

    [Test]
    public void SkipBlockComment_Returns_False()
    {
        var method = _type.GetMethod(
            "SkipBlockComment",
            BindingFlags.NonPublic | BindingFlags.Static)!;

        object[] args =
        {
            "SELECT *",
            0
        };

        var result = (bool)method.Invoke(
            null,
            args)!;

        Assert.That(result, Is.False);
    }

    #endregion

    #region ReadToken

    [Test]
    public void ReadToken_Returns_First_Token()
    {
        var method = _type.GetMethod(
            "ReadToken",
            BindingFlags.NonPublic | BindingFlags.Static)!;

        object[] args =
        {
            "SELECT * FROM CMS_User",
            0
        };

        var result = method.Invoke(null, args);

        Assert.That(result, Is.EqualTo("SELECT"));
    }

    [Test]
    public void ReadToken_Returns_Null_For_Numeric_Start()
    {
        var method = _type.GetMethod(
            "ReadToken",
            BindingFlags.NonPublic | BindingFlags.Static)!;

        object[] args =
        {
            "123SELECT",
            0
        };

        var result = method.Invoke(null, args);

        Assert.That(result, Is.Null);
    }

    #endregion

    #region Type Checks

    [Test]
    public void Type_EditQuery_Exists()
    {
        Assert.That(_type, Is.Not.Null);
    }

    [Test]
    public void GenerateQueryCodeName_Method_Exists()
    {
        Assert.That(
            _type.GetMethod(
                "GenerateQueryCodeName",
                BindingFlags.NonPublic | BindingFlags.Static),
            Is.Not.Null);
    }

    [Test]
    public void GetSavedQueries_Method_Exists()
    {
        Assert.That(
            _type.GetMethod(
                "GetSavedQueries",
                BindingFlags.NonPublic | BindingFlags.Instance),
            Is.Not.Null);
    }

    #endregion
}