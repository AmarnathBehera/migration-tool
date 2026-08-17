using BDO.GWT.CustomReportingModule;
using BDO.GWT.CustomReportingModule.ReportingApp;
using CMS.Base;
using CMS.Core;
using Moq;
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

    #region RunSql and Export tests

    private class FakeSqlProvider : ISqlBrowserResultProvider
    {
        public string? Query { get; private set; }
        public SqlBrowserQueryResult Result { get; set; } = new SqlBrowserQueryResult();

        public void SetQuery(string queryText) => Query = queryText;

        public string[] GetColumnNames() => Result.Columns;

        public Task<string> GetRowAsText(int rowIdentifier) => Task.FromResult(string.Empty);

        public int GetTotalRecordCount() => Result.Rows?.Length ?? 0;

        public string? GetQuery() => Query;

        public SqlBrowserQueryResult GetQueryResult() => Result;

        public Task<IEnumerable<dynamic>> GetRowsAsDynamic() => Task.FromResult(Enumerable.Empty<dynamic>());

        public Task<IEnumerable<IDataContainer>> GetRowsAsDataContainer() => Task.FromResult(Enumerable.Empty<IDataContainer>());

        IEnumerable<string> ISqlBrowserResultProvider.GetColumnNames()
        {
            return GetColumnNames();
        }
    }

    private object InvokeTaskResult(object instance, MethodInfo method, params object[] args)
    {
        var task = (Task)method.Invoke(instance, args)!;
        task.Wait();
        return task.GetType().GetProperty("Result")!.GetValue(task)!;
    }

    [Test]
    public void RunSql_Returns_Error_When_Query_Is_Empty()
    {
        var eventMock = new Mock<IEventLogService>();
        var exporterMock = new Mock<ISqlBrowserExporter>();
        var savedQueryMock = new Mock<CMS.DataEngine.IInfoProvider<ReportingReportInfo>>();
        var sqlProvider = new FakeSqlProvider();

        var instance = Activator.CreateInstance(
            _type,
            eventMock.Object,
            sqlProvider,
            exporterMock.Object,
            savedQueryMock.Object)!;

        var method = _type.GetMethod("RunSql")!;

        var response = InvokeTaskResult(instance, method, "")!;

        var value = response.GetType().GetProperty("Value")!.GetValue(response);
        Assert.That(value, Is.Not.Null);

        var error = value.GetType().GetProperty("ErrorMessage")!.GetValue(value) as string;
        Assert.That(error, Is.EqualTo("No query entered."));
    }

    [Test]
    public void RunSql_Disallows_Non_Select_Queries()
    {
        var eventMock = new Mock<IEventLogService>();
        var exporterMock = new Mock<ISqlBrowserExporter>();
        var savedQueryMock = new Mock<CMS.DataEngine.IInfoProvider<ReportingReportInfo>>();
        var sqlProvider = new FakeSqlProvider();

        var instance = Activator.CreateInstance(
            _type,
            eventMock.Object,
            sqlProvider,
            exporterMock.Object,
            savedQueryMock.Object)!;

        var method = _type.GetMethod("RunSql")!;

        var response = InvokeTaskResult(instance, method, "UPDATE Table SET Col = 1")!;

        var value = response.GetType().GetProperty("Value")!.GetValue(response);
        var error = value.GetType().GetProperty("ErrorMessage")!.GetValue(value) as string;

        Assert.That(error, Is.EqualTo("Only SELECT queries (optionally with CTEs) are allowed."));
    }

    [Test]
    public void RunSql_Allows_Select_With_Leading_Comments()
    {
        var eventMock = new Mock<IEventLogService>();
        var exporterMock = new Mock<ISqlBrowserExporter>();
        var savedQueryMock = new Mock<CMS.DataEngine.IInfoProvider<ReportingReportInfo>>();
        var sqlProvider = new FakeSqlProvider();
        sqlProvider.Result = new SqlBrowserQueryResult { ErrorMessage = null };

        var instance = Activator.CreateInstance(
            _type,
            eventMock.Object,
            sqlProvider,
            exporterMock.Object,
            savedQueryMock.Object)!;

        var method = _type.GetMethod("RunSql")!;

        var sql = "-- this is a comment\n/* block */\n   SELECT 1";

        var response = InvokeTaskResult(instance, method, sql)!;

        var messages = response.GetType().GetProperty("Messages")?.GetValue(response);
        // If Messages property exists, at least ensure the Value has no error
        var value = response.GetType().GetProperty("Value")!.GetValue(response);
        var error = value.GetType().GetProperty("ErrorMessage")!.GetValue(value) as string;

        Assert.That(error, Is.Null.Or.Empty);
    }

    [Test]
    public void ExportQuery_Returns_Error_When_Empty_Content()
    {
        var eventMock = new Mock<IEventLogService>();
        var exporterMock = new Mock<ISqlBrowserExporter>();
        exporterMock.Setup(x => x.Export(It.IsAny<SqlBrowserExportType>(), It.IsAny<string?>()))
            .ReturnsAsync((new byte[0], "", "text/csv"));

        var savedQueryMock = new Mock<CMS.DataEngine.IInfoProvider<ReportingReportInfo>>();
        var sqlProvider = new FakeSqlProvider();

        var instance = Activator.CreateInstance(
            _type,
            eventMock.Object,
            sqlProvider,
            exporterMock.Object,
            savedQueryMock.Object)!;

        var method = _type.GetMethod("ExportQuery")!;

        var response = InvokeTaskResult(instance, method, new object?[] { null })!;

        var value = response.GetType().GetProperty("Value")!.GetValue(response);
        Assert.That(value, Is.Null);
    }

    [Test]
    public void ExportQuery_Returns_Success_With_Content()
    {
        var eventMock = new Mock<IEventLogService>();
        var exporterMock = new Mock<ISqlBrowserExporter>();
        var content = System.Text.Encoding.UTF8.GetBytes("hello,world");
        exporterMock.Setup(x => x.Export(It.IsAny<SqlBrowserExportType>(), It.IsAny<string?>()))
            .ReturnsAsync((content, "my.csv", "text/csv"));

        var savedQueryMock = new Mock<CMS.DataEngine.IInfoProvider<ReportingReportInfo>>();
        var sqlProvider = new FakeSqlProvider();

        var instance = Activator.CreateInstance(
            _type,
            eventMock.Object,
            sqlProvider,
            exporterMock.Object,
            savedQueryMock.Object)!;

        var method = _type.GetMethod("ExportQuery")!;

        var response = InvokeTaskResult(instance, method, new object?[] { null })!;

        var value = response.GetType().GetProperty("Value")!.GetValue(response);
        Assert.That(value, Is.Not.Null);

        var base64 = (string)value.GetType().GetProperty("Base64")!.GetValue(value)!;
        var fileName = (string)value.GetType().GetProperty("FileName")!.GetValue(value)!;

        Assert.That(base64, Is.EqualTo(Convert.ToBase64String(content)));
        Assert.That(fileName, Is.EqualTo("my.csv"));
    }

    #endregion
}
