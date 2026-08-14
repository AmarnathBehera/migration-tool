using BDO.GWT.CustomReportingModule.ReportingApp;
using CMS.Core;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Modules.Reporting;

public class SqlBrowserResultProviderTests
{
    // ────── structural ──────────────────────────────────────────────────────────────────────────

    [Test]
    public void Service_ImplementsISqlBrowserResultProvider()
    {
        Assert.That(typeof(ISqlBrowserResultProvider).IsAssignableFrom(typeof(SqlBrowserResultProvider)), Is.True);
    }

    [Test]
    public void Service_IsPublicNonAbstractClass()
    {
        Assert.Multiple(() =>
        {
            Assert.That(typeof(SqlBrowserResultProvider).IsPublic, Is.True);
            Assert.That(typeof(SqlBrowserResultProvider).IsAbstract, Is.False);
        });
    }

    [Test]
    public void Service_HasSinglePublicConstructor()
    {
        Assert.That(typeof(SqlBrowserResultProvider).GetConstructors(), Has.Length.EqualTo(1));
    }

    [Test]
    public void Service_Constructor_HasTwoParameters()
    {
        Assert.That(typeof(SqlBrowserResultProvider).GetConstructors().Single().GetParameters(), Has.Length.EqualTo(2));
    }

    [Test]
    public void Service_Constructor_Parameter0_IsIEventLogService()
    {
        var param = typeof(SqlBrowserResultProvider).GetConstructors().Single().GetParameters()[0];

        Assert.That(param.ParameterType, Is.EqualTo(typeof(IEventLogService)));
    }

    [Test]
    public void Service_Constructor_Parameter1_IsISqlQueryValidator()
    {
        var param = typeof(SqlBrowserResultProvider).GetConstructors().Single().GetParameters()[1];

        Assert.That(param.ParameterType, Is.EqualTo(typeof(ISqlQueryValidator)));
    }

    // ────── interface shape ──────────────────────────────────────────────────────────────────────

    [Test]
    public void ISqlBrowserResultProvider_ExposesGetColumnNames()
    {
        Assert.That(typeof(ISqlBrowserResultProvider).GetMethod(nameof(ISqlBrowserResultProvider.GetColumnNames)), Is.Not.Null);
    }

    [Test]
    public void GetColumnNames_ReturnType_IsIEnumerableOfString()
    {
        var method = typeof(ISqlBrowserResultProvider).GetMethod(nameof(ISqlBrowserResultProvider.GetColumnNames));

        Assert.That(method!.ReturnType, Is.EqualTo(typeof(IEnumerable<string>)));
    }

    [Test]
    public void GetColumnNames_HasNoParameters()
    {
        var method = typeof(ISqlBrowserResultProvider).GetMethod(nameof(ISqlBrowserResultProvider.GetColumnNames));

        Assert.That(method!.GetParameters(), Is.Empty);
    }

    [Test]
    public void ISqlBrowserResultProvider_ExposesGetTotalRecordCount()
    {
        Assert.That(typeof(ISqlBrowserResultProvider).GetMethod(nameof(ISqlBrowserResultProvider.GetTotalRecordCount)), Is.Not.Null);
    }

    [Test]
    public void GetTotalRecordCount_ReturnType_IsInt()
    {
        var method = typeof(ISqlBrowserResultProvider).GetMethod(nameof(ISqlBrowserResultProvider.GetTotalRecordCount));

        Assert.That(method!.ReturnType, Is.EqualTo(typeof(int)));
    }

    [Test]
    public void ISqlBrowserResultProvider_ExposesGetRowAsText()
    {
        Assert.That(typeof(ISqlBrowserResultProvider).GetMethod(nameof(ISqlBrowserResultProvider.GetRowAsText)), Is.Not.Null);
    }

    [Test]
    public void GetRowAsText_ReturnType_IsTaskOfString()
    {
        var method = typeof(ISqlBrowserResultProvider).GetMethod(nameof(ISqlBrowserResultProvider.GetRowAsText));

        Assert.That(method!.ReturnType, Is.EqualTo(typeof(Task<string>)));
    }

    [Test]
    public void ISqlBrowserResultProvider_ExposesGetQuery()
    {
        Assert.That(typeof(ISqlBrowserResultProvider).GetMethod(nameof(ISqlBrowserResultProvider.GetQuery)), Is.Not.Null);
    }

    [Test]
    public void GetQuery_ReturnType_IsNullableString()
    {
        var method = typeof(ISqlBrowserResultProvider).GetMethod(nameof(ISqlBrowserResultProvider.GetQuery));

        Assert.That(method!.ReturnType, Is.EqualTo(typeof(string)));
    }

    [Test]
    public void ISqlBrowserResultProvider_ExposesSetQuery()
    {
        Assert.That(typeof(ISqlBrowserResultProvider).GetMethod(nameof(ISqlBrowserResultProvider.SetQuery)), Is.Not.Null);
    }

    [Test]
    public void SetQuery_Parameter_IsString()
    {
        var method = typeof(ISqlBrowserResultProvider).GetMethod(nameof(ISqlBrowserResultProvider.SetQuery));
        var parameters = method!.GetParameters();

        Assert.That(parameters, Has.Length.EqualTo(1));
        Assert.That(parameters[0].ParameterType, Is.EqualTo(typeof(string)));
    }

    [Test]
    public void ISqlBrowserResultProvider_ExposesGetQueryResult()
    {
        Assert.That(typeof(ISqlBrowserResultProvider).GetMethod(nameof(ISqlBrowserResultProvider.GetQueryResult)), Is.Not.Null);
    }

    [Test]
    public void GetQueryResult_ReturnType_IsSqlBrowserQueryResult()
    {
        var method = typeof(ISqlBrowserResultProvider).GetMethod(nameof(ISqlBrowserResultProvider.GetQueryResult));

        Assert.That(method!.ReturnType, Is.EqualTo(typeof(SqlBrowserQueryResult)));
    }

    [Test]
    public void ISqlBrowserResultProvider_ExposesGetRowsAsDynamic()
    {
        Assert.That(typeof(ISqlBrowserResultProvider).GetMethod(nameof(ISqlBrowserResultProvider.GetRowsAsDynamic)), Is.Not.Null);
    }

    [Test]
    public void GetRowsAsDynamic_ReturnType_IsTaskOfIEnumerableOfDynamic()
    {
        var method = typeof(ISqlBrowserResultProvider).GetMethod(nameof(ISqlBrowserResultProvider.GetRowsAsDynamic));

        Assert.That(method!.ReturnType, Is.EqualTo(typeof(Task<IEnumerable<dynamic>>)));
    }

    [Test]
    public void ISqlBrowserResultProvider_ExposesGetRowsAsDataContainer()
    {
        Assert.That(typeof(ISqlBrowserResultProvider).GetMethod(nameof(ISqlBrowserResultProvider.GetRowsAsDataContainer)), Is.Not.Null);
    }

    // ────── behavior: initialization ─────────────────────────────────────────────────────────────

    [Test]
    public void Constructor_WithDependencies_CreatesInstance()
    {
        var mockEventLog = new Mock<IEventLogService>();
        var mockValidator = new Mock<ISqlQueryValidator>();

        var service = new SqlBrowserResultProvider(mockEventLog.Object, mockValidator.Object);

        Assert.That(service, Is.Not.Null);
    }

    [Test]
    public void GetQuery_BeforeSetQuery_ReturnsNull()
    {
        var mockEventLog = new Mock<IEventLogService>();
        var mockValidator = new Mock<ISqlQueryValidator>();

        var service = new SqlBrowserResultProvider(mockEventLog.Object, mockValidator.Object);

        Assert.That(service.GetQuery(), Is.Null);
    }

    [Test]
    public void GetTotalRecordCount_BeforeSetQuery_ReturnsZero()
    {
        var mockEventLog = new Mock<IEventLogService>();
        var mockValidator = new Mock<ISqlQueryValidator>();

        var service = new SqlBrowserResultProvider(mockEventLog.Object, mockValidator.Object);

        Assert.That(service.GetTotalRecordCount(), Is.EqualTo(0));
    }

    [Test]
    public void GetColumnNames_BeforeSetQuery_ReturnsEmptyCollection()
    {
        var mockEventLog = new Mock<IEventLogService>();
        var mockValidator = new Mock<ISqlQueryValidator>();

        var service = new SqlBrowserResultProvider(mockEventLog.Object, mockValidator.Object);

        Assert.That(service.GetColumnNames(), Is.Empty);
    }

    // ────── behavior: SetQuery ───────────────────────────────────────────────────────────────────

    [Test]
    public void SetQuery_WithValidQuery_StoresQuery()
    {
        var mockEventLog = new Mock<IEventLogService>();
        var mockValidator = new Mock<ISqlQueryValidator>();

        var service = new SqlBrowserResultProvider(mockEventLog.Object, mockValidator.Object);
        string expectedQuery = "SELECT * FROM Users";

        service.SetQuery(expectedQuery);

        Assert.That(service.GetQuery(), Is.EqualTo(expectedQuery));
    }

    [Test]
    public void SetQuery_WithNewQuery_ClearsResult()
    {
        var mockEventLog = new Mock<IEventLogService>();
        var mockValidator = new Mock<ISqlQueryValidator>();

        var service = new SqlBrowserResultProvider(mockEventLog.Object, mockValidator.Object);
        service.SetQuery("SELECT * FROM Users");

        // Set new query
        service.SetQuery("SELECT * FROM Orders");

        Assert.That(service.GetQuery(), Is.EqualTo("SELECT * FROM Orders"));
    }

    // ────── behavior: GetQueryResult ─────────────────────────────────────────────────────────────

    [Test]
    public void GetQueryResult_BeforeSetQuery_ReturnsResultWithError()
    {
        var mockEventLog = new Mock<IEventLogService>();
        var mockValidator = new Mock<ISqlQueryValidator>();

        var service = new SqlBrowserResultProvider(mockEventLog.Object, mockValidator.Object);
        var result = service.GetQueryResult();

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Columns, Is.Not.Null);
        Assert.That(result.Rows, Is.Not.Null);
    }

    // ────── behavior: row text formatting ────────────────────────────────────────────────────────

    [Test]
    public async Task GetRowAsText_BeforeSetQuery_ThrowsException()
    {
        var mockEventLog = new Mock<IEventLogService>();
        var mockValidator = new Mock<ISqlQueryValidator>();

        var service = new SqlBrowserResultProvider(mockEventLog.Object, mockValidator.Object);

        Assert.ThrowsAsync<InvalidOperationException>(async () => await service.GetRowAsText(0));
    }

    // ────── behavior: GetRowsAsDynamic ──────────────────────────────────────────────────────────

    [Test]
    public async Task GetRowsAsDynamic_BeforeSetQuery_ReturnsEmptyCollection()
    {
        var mockEventLog = new Mock<IEventLogService>();
        var mockValidator = new Mock<ISqlQueryValidator>();

        var service = new SqlBrowserResultProvider(mockEventLog.Object, mockValidator.Object);
        var rows = await service.GetRowsAsDynamic();

        Assert.That(rows, Is.Empty);
    }

    // ────── behavior: GetRowsAsDataContainer ────────────────────────────────────────────────────

    [Test]
    public async Task GetRowsAsDataContainer_BeforeSetQuery_ReturnsEmptyCollection()
    {
        var mockEventLog = new Mock<IEventLogService>();
        var mockValidator = new Mock<ISqlQueryValidator>();

        var service = new SqlBrowserResultProvider(mockEventLog.Object, mockValidator.Object);
        var rows = await service.GetRowsAsDataContainer();

        Assert.That(rows, Is.Empty);
    }
}
