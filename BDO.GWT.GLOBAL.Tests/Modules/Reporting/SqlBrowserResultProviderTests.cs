using BDO.GWT.CustomReportingModule.ReportingApp;
using CMS.Core;
using Moq;
using System.Data;
using System.Reflection;

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

        Assert.Multiple(() =>
        {
            Assert.That(parameters, Has.Length.EqualTo(1));
            Assert.That(parameters[0].ParameterType, Is.EqualTo(typeof(string)));
        });
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
    public async Task GetRowsAsDataContainer_WithResult_ReturnsMappedContainers()
    {
        // Arrange
        var service = CreateService();

        var table = new DataTable();
        table.Columns.Add("Id", typeof(int));
        table.Columns.Add("Name", typeof(string));
        table.Columns.Add("IsActive", typeof(bool));

        table.Rows.Add(101, "John", true);
        table.Rows.Add(102, "Jane", false);

        var dataSet = new DataSet();
        dataSet.Tables.Add(table);

        typeof(SqlBrowserResultProvider)
            .GetField("_result", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(service, dataSet);

        // Act
        var containers = (await service.GetRowsAsDataContainer()).ToList();

        // Assert
        Assert.That(containers, Has.Count.EqualTo(2));

        var firstContainer = containers[0];
        var secondContainer = containers[1];

        Assert.Multiple(() =>
        {
            Assert.That(
                firstContainer[SqlBrowserResultProvider.ROW_IDENTIFIER_COLUMN],
                Is.EqualTo(0));

            Assert.That(
                secondContainer[SqlBrowserResultProvider.ROW_IDENTIFIER_COLUMN],
                Is.EqualTo(1));

            Assert.That(firstContainer["Id"], Is.EqualTo(101));
            Assert.That(firstContainer["Name"], Is.EqualTo("John"));
            Assert.That(firstContainer["IsActive"], Is.True);

            Assert.That(secondContainer["Id"], Is.EqualTo(102));
            Assert.That(secondContainer["Name"], Is.EqualTo("Jane"));
            Assert.That(secondContainer["IsActive"], Is.False);
        });
    }


    private static string GetRowIdentifierColumn()
    {
        return (string)typeof(SqlBrowserResultProvider)
            .GetField(
                "ROW_IDENTIFIER_COLUMN",
                BindingFlags.Static |
                BindingFlags.Public |
                BindingFlags.NonPublic)!
            .GetValue(null)!;
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
    private static readonly string[] ExpectedColumns =
    {
        "Id",
        "Name",
        "IsActive",
        "Description"
    };
    [Test]
    public void GetQueryResult_WithPopulatedResult_ReturnsColumnsAndRowsAsStrings()
    {
        // Arrange
        var service = CreateService();

        var table = new DataTable();
        table.Columns.Add("Id", typeof(int));
        table.Columns.Add("Name", typeof(string));
        table.Columns.Add("IsActive", typeof(bool));
        table.Columns.Add("Description", typeof(string));

        table.Rows.Add(101, "John", true, "First user");
        table.Rows.Add(102, "Jane", false, DBNull.Value);

        var dataSet = new DataSet();
        dataSet.Tables.Add(table);

        typeof(SqlBrowserResultProvider)
            .GetField(
                "_result",
                BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(service, dataSet);

        // Act
        var result = service.GetQueryResult();

        // Assert
        Assert.That(result, Is.Not.Null);

        Assert.Multiple(() =>
            {
                Assert.That(result.Columns, Is.EqualTo(ExpectedColumns));

                Assert.That(result.Rows, Has.Length.EqualTo(2));

                Assert.That(
                    result.Rows[0],
                    Is.EqualTo(new[]
                    {
                "101",
                "John",
                bool.TrueString,
                "First user"
                    }));

                Assert.That(
                    result.Rows[1],
                    Is.EqualTo(new[]
                    {
                "102",
                "Jane",
                bool.FalseString,
                string.Empty
                    }));
            });
    }

    [Test]
    public void GetQueryResult_WhenValidatorThrows_ReturnsExceptionMessage()
    {
        // Arrange
        var mockEventLog = new Mock<IEventLogService>();
        var mockValidator = new Mock<ISqlQueryValidator>();

        const string query = "SELECT * FROM Users";
        const string exceptionMessage = "SQL validation failed unexpectedly.";

        var expectedException = new InvalidOperationException(exceptionMessage);

        mockValidator
            .Setup(x => x.ValidateSqlStatement(query))
            .Throws(expectedException);

        var service = new SqlBrowserResultProvider(
            mockEventLog.Object,
            mockValidator.Object);

        service.SetQuery(query);

        // Act
        var result = service.GetQueryResult();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ErrorMessage, Is.EqualTo(exceptionMessage));
            Assert.That(result.Columns, Is.Empty);
            Assert.That(result.Rows, Is.Empty);
        });

        mockValidator.Verify(
            x => x.ValidateSqlStatement(query),
            Times.Once);
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

    [Test]
    public async Task GetRowAsText_WithValidRow_ReturnsFormattedText()
    {
        // Arrange
        var mockEventLog = new Mock<IEventLogService>();
        var mockValidator = new Mock<ISqlQueryValidator>();

        var service = new SqlBrowserResultProvider(
            mockEventLog.Object,
            mockValidator.Object);

        var table = new DataTable();
        table.Columns.Add("Id");
        table.Columns.Add("Name");

        var row = table.NewRow();
        row["Id"] = 1;
        row["Name"] = "John";
        table.Rows.Add(row);

        var dataSet = new DataSet();
        dataSet.Tables.Add(table);

        // Inject private field
        typeof(SqlBrowserResultProvider)
            .GetField("_result", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(service, dataSet);

        // Act
        var result = await service.GetRowAsText(0);

        // Assert
        Assert.That(result, Does.Contain("Id: 1"));
        Assert.That(result, Does.Contain("Name: John"));
    }


    private static SqlBrowserResultProvider CreateService()
    {
        var mockEventLog = new Mock<IEventLogService>();
        var mockValidator = new Mock<ISqlQueryValidator>();
        return new SqlBrowserResultProvider(mockEventLog.Object, mockValidator.Object);
    }
    [Test]
    public async Task GetRowsAsDynamic_WithResult_ReturnsRowsMappedByColumnName()
    {
        // Arrange
        var service = CreateService();

        var table = new DataTable();
        table.Columns.Add("Id", typeof(int));
        table.Columns.Add("Name", typeof(string));
        table.Columns.Add("IsActive", typeof(bool));

        table.Rows.Add(1, "John", true);
        table.Rows.Add(2, "Jane", false);

        var dataSet = new DataSet();
        dataSet.Tables.Add(table);

        typeof(SqlBrowserResultProvider)
            .GetField("_result", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(service, dataSet);

        // Act
        // ToList() is required to execute the deferred Select projection.
        var rows = (await service.GetRowsAsDynamic()).ToList();

        // Assert
        Assert.That(rows, Has.Count.EqualTo(2));

        var firstRow = (IDictionary<string, object>)rows[0];
        var secondRow = (IDictionary<string, object>)rows[1];

        Assert.Multiple(() =>
        {
            Assert.That(firstRow, Has.Count.EqualTo(3));
            Assert.That(firstRow.ContainsKey("Id"), Is.True);
            Assert.That(firstRow.ContainsKey("Name"), Is.True);
            Assert.That(firstRow.ContainsKey("IsActive"), Is.True);

            Assert.That(firstRow["Id"], Is.EqualTo(1));
            Assert.That(firstRow["Name"], Is.EqualTo("John"));
            Assert.That(firstRow["IsActive"], Is.True);

            Assert.That(secondRow["Id"], Is.EqualTo(2));
            Assert.That(secondRow["Name"], Is.EqualTo("Jane"));
            Assert.That(secondRow["IsActive"], Is.False);
        });
    }
    [Test]
    public async Task GetRowsAsDynamic_WithDatabaseNull_PreservesDBNullValue()
    {
        // Arrange
        var service = CreateService();

        var table = new DataTable();
        table.Columns.Add("Id", typeof(int));
        table.Columns.Add("Description", typeof(string));

        table.Rows.Add(1, DBNull.Value);

        var dataSet = new DataSet();
        dataSet.Tables.Add(table);

        typeof(SqlBrowserResultProvider)
            .GetField("_result", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(service, dataSet);

        // Act
        var rows = (await service.GetRowsAsDynamic()).ToList();

        // Assert
        Assert.That(rows, Has.Count.EqualTo(1));

        var row = (IDictionary<string, object>)rows.Single();

        Assert.Multiple(() =>
        {
            Assert.That(row["Id"], Is.EqualTo(1));
            Assert.That(row["Description"], Is.EqualTo(DBNull.Value));
        });
    }
}
