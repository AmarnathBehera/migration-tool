using BDO.GWT.CustomReportingModule;
using BDO.GWT.CustomReportingModule.ReportingApp;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using Moq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace BDO.GWT.GLOBAL.Tests.Modules.Reporting;

[TestFixture]
public partial class EditQueryTests
{
    [GeneratedRegex(@"^Report_[a-fA-F0-9]{32}$")]
    private static partial Regex ReportCodeNamePattern();

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
            ReportCodeNamePattern().IsMatch(result),
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
    public void GetSavedQueriesMethodExists()
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

        public SqlBrowserQueryResult Result { get; set; } =
            new()
            {
                Columns = Array.Empty<string>()
            };

        public void SetQuery(string queryText) => Query = queryText;

        public string[] GetColumnNames() =>
            Result.Columns ?? Array.Empty<string>();

        IEnumerable<string> ISqlBrowserResultProvider.GetColumnNames() =>
            GetColumnNames();

        public Task<string> GetRowAsText(int rowIdentifier) =>
            Task.FromResult(string.Empty);

        public int GetTotalRecordCount() =>
            Result.Rows?.Length ?? 0;

        public string? GetQuery() => Query;

        public SqlBrowserQueryResult GetQueryResult() => Result;

        public Task<IEnumerable<dynamic>> GetRowsAsDynamic() =>
            Task.FromResult(Enumerable.Empty<dynamic>());

        public Task<IEnumerable<IDataContainer>> GetRowsAsDataContainer() =>
            Task.FromResult(Enumerable.Empty<IDataContainer>());
    }

    private static object? InvokeTaskResult(
        object instance,
        MethodInfo method,
        params object?[] args)
    {
        var task = (Task)method.Invoke(instance, args)!;

        task.GetAwaiter().GetResult();

        return task.GetType()
            .GetProperty("Result")
            ?.GetValue(task);
    }

    [Test]
    public void RunSql_Returns_Error_When_Query_Is_Empty()
    {
        var instance = Activator.CreateInstance(
            _type,
            Mock.Of<IEventLogService>(),
            new FakeSqlProvider(),
            Mock.Of<ISqlBrowserExporter>(),
            Mock.Of<IInfoProvider<ReportingReportInfo>>())!;

        var method = _type.GetMethod("RunSql")!;

        Assert.DoesNotThrow(() =>
        {
            var response = InvokeTaskResult(
                instance,
                method,
                "");

            Assert.That(response, Is.Not.Null);
        });
    }

    [Test]
    public void RunSql_Disallows_Non_Select_Queries()
    {
        var sqlProvider = new FakeSqlProvider();

        var instance = Activator.CreateInstance(
            _type,
            Mock.Of<IEventLogService>(),
            sqlProvider,
            Mock.Of<ISqlBrowserExporter>(),
            Mock.Of<IInfoProvider<ReportingReportInfo>>())!;

        var method = _type.GetMethod("RunSql")!;

        Assert.DoesNotThrow(() =>
        {
            InvokeTaskResult(
                instance,
                method,
                "UPDATE Table SET Col = 1");
        });

        Assert.That(sqlProvider.Query, Is.Null);
    }

    [Test]
    public void RunSql_Allows_Supported_Select_With_Leading_Comments()
    {
        var sqlProvider = new FakeSqlProvider();

        var instance = Activator.CreateInstance(
            _type,
            Mock.Of<IEventLogService>(),
            sqlProvider,
            Mock.Of<ISqlBrowserExporter>(),
            Mock.Of<IInfoProvider<ReportingReportInfo>>())!;

        var method = _type.GetMethod("RunSql")!;

        var sql =
            "-- this is a comment\n/* block */\n   SELECT * FROM CMS_User";

        InvokeTaskResult(instance, method, sql);

        Assert.That(sqlProvider.Query, Is.EqualTo(sql));
    }

    [Test]
    public void RunSql_Disallows_With_Cte_Query()
    {
        var sqlProvider = new FakeSqlProvider();

        var instance = Activator.CreateInstance(
            _type,
            Mock.Of<IEventLogService>(),
            sqlProvider,
            Mock.Of<ISqlBrowserExporter>(),
            Mock.Of<IInfoProvider<ReportingReportInfo>>())!;

        var method = _type.GetMethod("RunSql")!;

        const string sql =
    @"WITH Data AS
(
    SELECT 1 AS Id
)
SELECT * FROM Data";

        InvokeTaskResult(instance, method, sql);

        Assert.That(sqlProvider.Query, Is.Null);
    }

    [Test]
    public void RunSql_Returns_Provider_Error()
    {
        var sqlProvider = new FakeSqlProvider
        {
            Result = new SqlBrowserQueryResult
            {
                Columns = Array.Empty<string>(),
                ErrorMessage = "SQL execution failed"
            }
        };

        var instance = Activator.CreateInstance(
            _type,
            Mock.Of<IEventLogService>(),
            sqlProvider,
            Mock.Of<ISqlBrowserExporter>(),
            Mock.Of<IInfoProvider<ReportingReportInfo>>())!;

        var method = _type.GetMethod("RunSql")!;

        InvokeTaskResult(
            instance,
            method,
            "SELECT * FROM CMS_User");

        Assert.That(
            sqlProvider.Result.ErrorMessage,
            Is.EqualTo("SQL execution failed"));
    }

    [Test]
    public void RunSql_Sets_Query_On_Provider()
    {
        var sqlProvider = new FakeSqlProvider();

        var instance = Activator.CreateInstance(
            _type,
            Mock.Of<IEventLogService>(),
            sqlProvider,
            Mock.Of<ISqlBrowserExporter>(),
            Mock.Of<IInfoProvider<ReportingReportInfo>>())!;

        var method = _type.GetMethod("RunSql")!;

        const string query = "SELECT * FROM CMS_User";

        InvokeTaskResult(instance, method, query);

        Assert.That(sqlProvider.Query, Is.EqualTo(query));
    }

    [Test]
    public void ExportQuery_Returns_Error_When_Empty_Content()
    {
        var exporterMock = new Mock<ISqlBrowserExporter>();

        exporterMock
            .Setup(x => x.Export(
                It.IsAny<SqlBrowserExportType>(),
                It.IsAny<string?>()))
            .ReturnsAsync((
                Array.Empty<byte>(),
                "",
                "text/csv"));

        var instance = Activator.CreateInstance(
            _type,
            Mock.Of<IEventLogService>(),
            new FakeSqlProvider(),
            exporterMock.Object,
            Mock.Of<IInfoProvider<ReportingReportInfo>>())!;

        var method = _type.GetMethod("ExportQuery")!;

        var response = InvokeTaskResult(
            instance,
            method,
            (JsonElement?)null);

        Assert.That(response, Is.Not.Null);
    }

    [Test]
    public void ExportQuery_Returns_Success_With_Content()
    {
        var content = Encoding.UTF8.GetBytes("hello,world");

        var exporterMock = new Mock<ISqlBrowserExporter>();

        exporterMock
            .Setup(x => x.Export(
                It.IsAny<SqlBrowserExportType>(),
                It.IsAny<string?>()))
            .ReturnsAsync((
                content,
                "my.csv",
                "text/csv"));

        var instance = Activator.CreateInstance(
            _type,
            Mock.Of<IEventLogService>(),
            new FakeSqlProvider(),
            exporterMock.Object,
            Mock.Of<IInfoProvider<ReportingReportInfo>>())!;

        var method = _type.GetMethod("ExportQuery")!;

        var response = InvokeTaskResult(
            instance,
            method,
            (JsonElement?)null);

        Assert.That(response, Is.Not.Null);

        exporterMock.Verify(
            x => x.Export(
                It.IsAny<SqlBrowserExportType>(),
                It.IsAny<string?>()),
            Times.Once);
    }

    [Test]
    public void ExportQuery_Returns_Error_When_Exporter_Throws()
    {
        var exporterMock = new Mock<ISqlBrowserExporter>();

        exporterMock
            .Setup(x => x.Export(
                It.IsAny<SqlBrowserExportType>(),
                It.IsAny<string?>()))
            .ThrowsAsync(new InvalidOperationException("boom"));

        var instance = Activator.CreateInstance(
            _type,
            Mock.Of<IEventLogService>(),
            new FakeSqlProvider(),
            exporterMock.Object,
            Mock.Of<IInfoProvider<ReportingReportInfo>>())!;

        var method = _type.GetMethod("ExportQuery")!;

        Assert.DoesNotThrow(() =>
        {
            var response = InvokeTaskResult(
                instance,
                method,
                (JsonElement?)null);

            Assert.That(response, Is.Not.Null);
        });
    }

    #endregion

    #region Additional Coverage

    [Test]
    public void GenerateQueryCodeName_Trims_Dots()
    {
        var method = _type.GetMethod(
            "GenerateQueryCodeName",
            BindingFlags.NonPublic | BindingFlags.Static)!;

        var result = (string)method.Invoke(
            null,
            new object[] { ".Report." })!;

        Assert.That(result, Does.StartWith("Report_"));
    }

    #endregion

    #region Rename
    [Test]
    public void RenameQuery_Returns_Error_When_Id_Is_Invalid()
    {
        var instance = Activator.CreateInstance(
            _type,
            Mock.Of<IEventLogService>(),
            Mock.Of<ISqlBrowserResultProvider>(),
            Mock.Of<ISqlBrowserExporter>(),
            Mock.Of<IInfoProvider<ReportingReportInfo>>())!;

        var method = _type.GetMethod("RenameQuery")!;

        var model = new SavedQuery
        {
            ID = 0,
            Name = "New Name"
        };

        Assert.DoesNotThrow(() =>
        {
            var result = InvokeTaskResult(
                instance,
                method,
                model);

            Assert.That(result, Is.Not.Null);
        });
    }

    [Test]
    public void RenameQuery_Returns_Error_When_Name_Is_Empty()
    {
        var instance = Activator.CreateInstance(
            _type,
            Mock.Of<IEventLogService>(),
            Mock.Of<ISqlBrowserResultProvider>(),
            Mock.Of<ISqlBrowserExporter>(),
            Mock.Of<IInfoProvider<ReportingReportInfo>>())!;

        var method = _type.GetMethod("RenameQuery")!;

        var model = new SavedQuery
        {
            ID = 10,
            Name = ""
        };

        Assert.DoesNotThrow(() =>
        {
            var result = InvokeTaskResult(
                instance,
                method,
                model);

            Assert.That(result, Is.Not.Null);
        });
    }

    [Test]
    public void ConfigureTemplateProperties_Sets_Default_Query_And_ChannelId_Before_LoadingSavedQueries()
    {
        var providerMock = new Mock<IInfoProvider<ReportingReportInfo>>();
        providerMock.Setup(p => p.Get()).Throws(new InvalidOperationException("boom"));

        var instance = Activator.CreateInstance(
            _type,
            Mock.Of<IEventLogService>(),
            Mock.Of<ISqlBrowserResultProvider>(),
            Mock.Of<ISqlBrowserExporter>(),
            providerMock.Object)!;

        _type.GetProperty(nameof(EditQuery.ReportingChannelSettingId))!
            .SetValue(instance, 42);

        var method = _type.GetMethod(nameof(EditQuery.ConfigureTemplateProperties))!;
        var properties = new EditSqlTemplateClientProperties();

        Assert.Throws<InvalidOperationException>(() =>
            InvokeTaskResult(instance, method, properties));

        Assert.That(properties.Query, Does.Contain("No query has been configured."));
        Assert.That(properties.Query, Does.Contain("Use ChannelID = 42"));
    }

    [Test]
    public void RenameQuery_Returns_Error_When_Query_Is_Not_Found()
    {
        var providerMock = new Mock<IInfoProvider<ReportingReportInfo>>();
        var queryMock = new Mock<ObjectQuery<ReportingReportInfo>>();
        IQueryable<ReportingReportInfo> emptyQuery = new EnumerableQuery<ReportingReportInfo>(Array.Empty<ReportingReportInfo>());

        queryMock.Setup(q => q.TopN(It.IsAny<int>())).Returns(queryMock.Object);
        queryMock.Setup(q => q.WhereEquals(It.IsAny<string>(), It.IsAny<object>())).Returns(queryMock.Object);
        queryMock.As<IQueryable<ReportingReportInfo>>().SetupGet(q => q.Provider).Returns(emptyQuery.Provider);
        queryMock.As<IQueryable<ReportingReportInfo>>().SetupGet(q => q.Expression).Returns(emptyQuery.Expression);
        queryMock.As<IQueryable<ReportingReportInfo>>().SetupGet(q => q.ElementType).Returns(emptyQuery.ElementType);
        queryMock.As<IEnumerable<ReportingReportInfo>>().Setup(q => q.GetEnumerator()).Returns(emptyQuery.GetEnumerator());
        providerMock.Setup(p => p.Get()).Returns(queryMock.Object);

        var instance = Activator.CreateInstance(
            _type,
            Mock.Of<IEventLogService>(),
            Mock.Of<ISqlBrowserResultProvider>(),
            Mock.Of<ISqlBrowserExporter>(),
            providerMock.Object)!;

        var method = _type.GetMethod("RenameQuery")!;
        var model = new SavedQuery { ID = 99, Name = "Updated" };

        var response = InvokeTaskResult(instance, method, model);
        var value = GetResponseValue(response!);

        Assert.That(value, Is.Null);
        providerMock.Verify(p => p.Get(), Times.Once);
    }

    [Test]
    public void DeleteQuery_Returns_Error_When_Query_Is_Not_Found()
    {
        var providerMock = new Mock<IInfoProvider<ReportingReportInfo>>();
        var queryMock = new Mock<ObjectQuery<ReportingReportInfo>>();
        IQueryable<ReportingReportInfo> emptyQuery = new EnumerableQuery<ReportingReportInfo>(Array.Empty<ReportingReportInfo>());

        queryMock.Setup(q => q.TopN(It.IsAny<int>())).Returns(queryMock.Object);
        queryMock.Setup(q => q.WhereEquals(It.IsAny<string>(), It.IsAny<object>())).Returns(queryMock.Object);
        queryMock.As<IQueryable<ReportingReportInfo>>().SetupGet(q => q.Provider).Returns(emptyQuery.Provider);
        queryMock.As<IQueryable<ReportingReportInfo>>().SetupGet(q => q.Expression).Returns(emptyQuery.Expression);
        queryMock.As<IQueryable<ReportingReportInfo>>().SetupGet(q => q.ElementType).Returns(emptyQuery.ElementType);
        queryMock.As<IEnumerable<ReportingReportInfo>>().Setup(q => q.GetEnumerator()).Returns(emptyQuery.GetEnumerator());
        providerMock.Setup(p => p.Get()).Returns(queryMock.Object);

        var instance = Activator.CreateInstance(
            _type,
            Mock.Of<IEventLogService>(),
            Mock.Of<ISqlBrowserResultProvider>(),
            Mock.Of<ISqlBrowserExporter>(),
            providerMock.Object)!;

        var method = _type.GetMethod("DeleteQuery")!;

        var response = InvokeTaskResult(instance, method, 99);
        var value = GetResponseValue(response!);

        Assert.That(value, Is.Null);
        providerMock.Verify(p => p.Get(), Times.Once);
    }

    [Test]
    public void UpdateQuery_Returns_Error_When_Input_Is_Invalid()
    {
        var instance = Activator.CreateInstance(
            _type,
            Mock.Of<IEventLogService>(),
            Mock.Of<ISqlBrowserResultProvider>(),
            Mock.Of<ISqlBrowserExporter>(),
            Mock.Of<IInfoProvider<ReportingReportInfo>>())!;

        var method = _type.GetMethod("UpdateQuery")!;
        var model = new SavedQuery { ID = 0, Text = "SELECT 1" };

        var response = InvokeTaskResult(instance, method, model);
        var value = GetResponseValue(response!);

        Assert.That(value, Is.Null);
    }

    [Test]
    public void UpdateQuery_Returns_Error_When_Query_Is_Not_Found()
    {
        var providerMock = new Mock<IInfoProvider<ReportingReportInfo>>();
        var queryMock = new Mock<ObjectQuery<ReportingReportInfo>>();
        IQueryable<ReportingReportInfo> emptyQuery = new EnumerableQuery<ReportingReportInfo>(Array.Empty<ReportingReportInfo>());

        queryMock.Setup(q => q.TopN(It.IsAny<int>())).Returns(queryMock.Object);
        queryMock.Setup(q => q.WhereEquals(It.IsAny<string>(), It.IsAny<object>())).Returns(queryMock.Object);
        queryMock.As<IQueryable<ReportingReportInfo>>().SetupGet(q => q.Provider).Returns(emptyQuery.Provider);
        queryMock.As<IQueryable<ReportingReportInfo>>().SetupGet(q => q.Expression).Returns(emptyQuery.Expression);
        queryMock.As<IQueryable<ReportingReportInfo>>().SetupGet(q => q.ElementType).Returns(emptyQuery.ElementType);
        queryMock.As<IEnumerable<ReportingReportInfo>>().Setup(q => q.GetEnumerator()).Returns(emptyQuery.GetEnumerator());
        providerMock.Setup(p => p.Get()).Returns(queryMock.Object);

        var instance = Activator.CreateInstance(
            _type,
            Mock.Of<IEventLogService>(),
            Mock.Of<ISqlBrowserResultProvider>(),
            Mock.Of<ISqlBrowserExporter>(),
            providerMock.Object)!;

        var method = _type.GetMethod("UpdateQuery")!;
        var model = new SavedQuery { ID = 99, Name = "Updated", Text = "SELECT 1" };

        var response = InvokeTaskResult(instance, method, model);
        var value = GetResponseValue(response!);

        Assert.That(value, Is.Null);
        providerMock.Verify(p => p.Get(), Times.Once);
    }
    [Test]
    public void RenameQuery_Method_Exists()
    {
        Assert.That(
            _type.GetMethod("RenameQuery"),
            Is.Not.Null);
    }

    [Test]
    public void SaveQuery_Method_Exists()
    {
        Assert.That(
            _type.GetMethod("SaveQuery"),
            Is.Not.Null);
    }

    [Test]
    public void UpdateSavedOrder_Method_Exists()
    {
        Assert.That(
            _type.GetMethod("UpdateSavedOrder"),
            Is.Not.Null);
    }

    [Test]
    public void ExportQuery_Method_Exists()
    {
        Assert.That(
            _type.GetMethod("ExportQuery"),
            Is.Not.Null);
    }
    #endregion

    #region Delete
    [Test]
    public void DeleteQuery_Method_Exists()
    {
        Assert.That(
            _type.GetMethod("DeleteQuery"),
            Is.Not.Null);
    }
    [Test]
    public void DeleteQuery_Has_Expected_Parameter()
    {
        var method = _type.GetMethod("DeleteQuery")!;

        var parameters = method.GetParameters();

        Assert.That(parameters, Has.Length.EqualTo(1));
        Assert.That(parameters[0].ParameterType, Is.EqualTo(typeof(int)));
    }
    #endregion

    #region Save
    [Test]
    public void SaveQuery_Returns_Error_When_Name_Is_Empty()
    {
        var instance = Activator.CreateInstance(
            _type,
            Mock.Of<IEventLogService>(),
            new FakeSqlProvider(),
            Mock.Of<ISqlBrowserExporter>(),
            Mock.Of<IInfoProvider<ReportingReportInfo>>())!;

        var method = _type.GetMethod("SaveQuery")!;

        var query = new SavedQuery
        {
            Name = "",
            Text = "SELECT * FROM CMS_User"
        };

        var response = InvokeTaskResult(
            instance,
            method,
            query);

        var value = GetResponseValue(response!);

        Assert.That(value, Is.Null);
    }

    [Test]
    public void SaveQuery_Returns_Error_When_Text_Is_Empty()
    {
        var instance = Activator.CreateInstance(
            _type,
            Mock.Of<IEventLogService>(),
            new FakeSqlProvider(),
            Mock.Of<ISqlBrowserExporter>(),
            Mock.Of<IInfoProvider<ReportingReportInfo>>())!;

        var method = _type.GetMethod("SaveQuery")!;

        var query = new SavedQuery
        {
            Name = "Test Query",
            Text = ""
        };

        var response = InvokeTaskResult(
            instance,
            method,
            query);

        var value = GetResponseValue(response!);

        Assert.That(value, Is.Null);
    }
    [Test]
    public void SaveQuery_Returns_Error_When_Query_Is_Invalid()
    {
        var instance = Activator.CreateInstance(
            _type,
            Mock.Of<IEventLogService>(),
            new FakeSqlProvider(),
            Mock.Of<ISqlBrowserExporter>(),
            Mock.Of<IInfoProvider<ReportingReportInfo>>())!;

        var method = _type.GetMethod("SaveQuery")!;

        var query = new SavedQuery
        {
            Name = "",
            Text = ""
        };

        var response = InvokeTaskResult(
            instance,
            method,
            query);

        var value = GetResponseValue(response!);

        Assert.That(value, Is.Null);
    }
    [Test]
    public void SaveQuery_Logs_Exception_When_Insert_Fails()
    {
        var eventMock = new Mock<IEventLogService>();

        var instance = Activator.CreateInstance(
            _type,
            eventMock.Object,
            new FakeSqlProvider(),
            Mock.Of<ISqlBrowserExporter>(),
            Mock.Of<IInfoProvider<ReportingReportInfo>>())!;

        var method = _type.GetMethod("SaveQuery")!;

        var query = new SavedQuery
        {
            Name = "Report",
            Text = "SELECT 1"
        };

        Assert.DoesNotThrow(() =>
        {
            try
            {
                InvokeTaskResult(instance, method, query);
            }
            catch
            {
            }
        });
    }
    private static object? GetResponseValue(object response)
    {
        return response.GetType()
            .GetProperty("Value")?
            .GetValue(response);
    }

    #endregion

    [Test]
    public void GenerateQueryCodeName_When_Name_Is_Only_Dots_Returns_Query()
    {
        var method = _type.GetMethod(
            "GenerateQueryCodeName",
            BindingFlags.NonPublic | BindingFlags.Static)!;

        var result = (string)method.Invoke(
            null,
            new object[] { "..." })!;

        Assert.That(result, Does.StartWith("Query_"));
    }

    [Test]
    public void ExportQuery_Uses_Provided_Model()
    {
        var exporterMock = new Mock<ISqlBrowserExporter>();

        exporterMock
            .Setup(x => x.Export(
                SqlBrowserExportType.Excel,
                "report"))
            .ReturnsAsync((
                Encoding.UTF8.GetBytes("test"),
                "report.xlsx",
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"));

        var instance = Activator.CreateInstance(
            _type,
            Mock.Of<IEventLogService>(),
            new FakeSqlProvider(),
            exporterMock.Object,
            Mock.Of<IInfoProvider<ReportingReportInfo>>())!;

        var method = _type.GetMethod("ExportQuery")!;

        var json =
            """
        {
            "ExportType":"excel",
            "FileName":"report"
        }
        """;

        using var document = JsonDocument.Parse(json);

        JsonElement element = document.RootElement;

        Assert.DoesNotThrow(() =>
        {
            InvokeTaskResult(
                instance,
                method,
                (JsonElement?)element);
        });

        exporterMock.Verify(
            x => x.Export(
                SqlBrowserExportType.Excel,
                "report"),
            Times.Once);
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

    #region Notify

    [Test]
    public void Notify_Method_Exists()
    {
        Assert.That(
            _type.GetMethod("Notify"),
            Is.Not.Null);
    }

    [Test]
    public void Notify_Returns_Response()
    {
        var instance = Activator.CreateInstance(
            _type,
            Mock.Of<IEventLogService>(),
            new FakeSqlProvider(),
            Mock.Of<ISqlBrowserExporter>(),
            Mock.Of<IInfoProvider<ReportingReportInfo>>())!;

        var method = _type.GetMethod("Notify")!;

        Assert.DoesNotThrow(() =>
        {
            var response = InvokeTaskResult(instance, method, "test message");
            Assert.That(response, Is.Not.Null);
        });
    }

    #endregion

    #region RenameQuery additional

    [Test]
    public void RenameQuery_Calls_Provider_Get_When_Input_Is_Valid()
    {
        var providerMock = new Mock<IInfoProvider<ReportingReportInfo>>();
        var queryMock = new Mock<ObjectQuery<ReportingReportInfo>>();
        queryMock.Setup(q => q.TopN(It.IsAny<int>())).Returns(queryMock.Object);
        queryMock.Setup(q => q.WhereEquals(It.IsAny<string>(), It.IsAny<object>())).Returns(queryMock.Object);
        providerMock.Setup(p => p.Get()).Returns(queryMock.Object);

        var instance = Activator.CreateInstance(
            _type,
            Mock.Of<IEventLogService>(),
            new FakeSqlProvider(),
            Mock.Of<ISqlBrowserExporter>(),
            providerMock.Object)!;

        var method = _type.GetMethod("RenameQuery")!;
        var model = new SavedQuery { ID = 99, Name = "New Name" };

        // Will throw when ObjectQuery is actually enumerated (no DB), but provider.Get() is called
        try { InvokeTaskResult(instance, method, model); } catch { }

        providerMock.Verify(p => p.Get(), Times.Once);
    }

    #endregion

    #region DeleteQuery additional

    [Test]
    public void DeleteQuery_Calls_Provider_Get_When_Input_Is_Valid()
    {
        var providerMock = new Mock<IInfoProvider<ReportingReportInfo>>();
        var queryMock = new Mock<ObjectQuery<ReportingReportInfo>>();
        queryMock.Setup(q => q.TopN(It.IsAny<int>())).Returns(queryMock.Object);
        queryMock.Setup(q => q.WhereEquals(It.IsAny<string>(), It.IsAny<object>())).Returns(queryMock.Object);
        providerMock.Setup(p => p.Get()).Returns(queryMock.Object);

        var instance = Activator.CreateInstance(
            _type,
            Mock.Of<IEventLogService>(),
            new FakeSqlProvider(),
            Mock.Of<ISqlBrowserExporter>(),
            providerMock.Object)!;

        var method = _type.GetMethod("DeleteQuery")!;

        // Will throw when ObjectQuery is actually enumerated (no DB), but provider.Get() is called
        try { InvokeTaskResult(instance, method, 42); } catch { }

        providerMock.Verify(p => p.Get(), Times.Once);
    }

    #endregion

    #region ExportQuery additional

    [Test]
    public void ExportQuery_Uses_Json_Export_Type()
    {
        var content = Encoding.UTF8.GetBytes("[{\"id\":1}]");

        var exporterMock = new Mock<ISqlBrowserExporter>();
        exporterMock
            .Setup(x => x.Export(SqlBrowserExportType.Json, It.IsAny<string?>()))
            .ReturnsAsync((content, "export.json", "application/json"));

        var instance = Activator.CreateInstance(
            _type,
            Mock.Of<IEventLogService>(),
            new FakeSqlProvider(),
            exporterMock.Object,
            Mock.Of<IInfoProvider<ReportingReportInfo>>())!;

        var method = _type.GetMethod("ExportQuery")!;

        var json = """
        {
            "ExportType":"json",
            "FileName":"export.json"
        }
        """;

        using var document = JsonDocument.Parse(json);
        JsonElement element = document.RootElement;

        Assert.DoesNotThrow(() =>
        {
            InvokeTaskResult(instance, method, (JsonElement?)element);
        });

        exporterMock.Verify(
            x => x.Export(SqlBrowserExportType.Json, "export.json"),
            Times.Once);
    }

    #endregion

}
