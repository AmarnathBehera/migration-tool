using BDO.GWT.CustomReportingModule.ReportingApp;
using Microsoft.AspNetCore.Hosting;
using Moq;
using System.Text;

namespace BDO.GWT.GLOBAL.Tests.Modules.Reporting;

public class SqlBrowserExporterTests
{
    // ────── structural ──────────────────────────────────────────────────────────────────────────

    [Test]
    public void Service_ImplementsISqlBrowserExporter()
    {
        Assert.That(typeof(ISqlBrowserExporter).IsAssignableFrom(typeof(SqlBrowserExporter)), Is.True);
    }

    [Test]
    public void Service_IsPublicNonAbstractClass()
    {
        Assert.Multiple(() =>
        {
            Assert.That(typeof(SqlBrowserExporter).IsPublic, Is.True);
            Assert.That(typeof(SqlBrowserExporter).IsAbstract, Is.False);
        });
    }

    [Test]
    public void Service_HasSinglePublicConstructor()
    {
        Assert.That(typeof(SqlBrowserExporter).GetConstructors(), Has.Length.EqualTo(1));
    }

    [Test]
    public void Service_Constructor_HasTwoParameters()
    {
        Assert.That(typeof(SqlBrowserExporter).GetConstructors().Single().GetParameters(), Has.Length.EqualTo(2));
    }

    [Test]
    public void Service_Constructor_Parameter0_IsIWebHostEnvironment()
    {
        var param = typeof(SqlBrowserExporter).GetConstructors().Single().GetParameters()[0];

        Assert.That(param.ParameterType, Is.EqualTo(typeof(IWebHostEnvironment)));
    }

    [Test]
    public void Service_Constructor_Parameter1_IsISqlBrowserResultProvider()
    {
        var param = typeof(SqlBrowserExporter).GetConstructors().Single().GetParameters()[1];

        Assert.That(param.ParameterType, Is.EqualTo(typeof(ISqlBrowserResultProvider)));
    }

    // ────── interface shape ──────────────────────────────────────────────────────────────────────

    [Test]
    public void ISqlBrowserExporter_ExposesExport()
    {
        Assert.That(typeof(ISqlBrowserExporter).GetMethod(nameof(ISqlBrowserExporter.Export)), Is.Not.Null);
    }

    [Test]
    public void Export_ReturnType_IsTaskOfTuple()
    {
        var method = typeof(ISqlBrowserExporter).GetMethod(nameof(ISqlBrowserExporter.Export));

        // Task<(byte[] Content, string FileName, string ContentType)>
        Assert.That(method!.ReturnType, Is.EqualTo(typeof(Task<(byte[], string, string)>)));
    }

    [Test]
    public void Export_HasTwoParameters()
    {
        var method = typeof(ISqlBrowserExporter).GetMethod(nameof(ISqlBrowserExporter.Export));

        Assert.That(method!.GetParameters(), Has.Length.EqualTo(2));
    }

    [Test]
    public void Export_Parameter0_IsSqlBrowserExportType()
    {
        var method = typeof(ISqlBrowserExporter).GetMethod(nameof(ISqlBrowserExporter.Export));
        var param = method!.GetParameters()[0];

        Assert.That(param.ParameterType, Is.EqualTo(typeof(SqlBrowserExportType)));
    }

    [Test]
    public void Export_Parameter1_IsNullableString()
    {
        var method = typeof(ISqlBrowserExporter).GetMethod(nameof(ISqlBrowserExporter.Export));
        var param = method!.GetParameters()[1];

        Assert.That(param.ParameterType, Is.EqualTo(typeof(string)));
    }

    [Test]
    public void ISqlBrowserExporter_ExposesGetExportDirectory()
    {
        Assert.That(typeof(ISqlBrowserExporter).GetMethod(nameof(ISqlBrowserExporter.GetExportDirectory)), Is.Not.Null);
    }

    [Test]
    public void GetExportDirectory_ReturnType_IsString()
    {
        var method = typeof(ISqlBrowserExporter).GetMethod(nameof(ISqlBrowserExporter.GetExportDirectory));

        Assert.That(method!.ReturnType, Is.EqualTo(typeof(string)));
    }

    [Test]
    public void GetExportDirectory_HasNoParameters()
    {
        var method = typeof(ISqlBrowserExporter).GetMethod(nameof(ISqlBrowserExporter.GetExportDirectory));

        Assert.That(method!.GetParameters(), Is.Empty);
    }

    // ────── behavior: initialization ─────────────────────────────────────────────────────────────

    [Test]
    public void Constructor_WithDependencies_CreatesInstance()
    {
        var mockEnv = new Mock<IWebHostEnvironment>();
        var mockResultProvider = new Mock<ISqlBrowserResultProvider>();

        var service = new SqlBrowserExporter(mockEnv.Object, mockResultProvider.Object);

        Assert.That(service, Is.Not.Null);
    }

    // ────── behavior: GetExportDirectory ─────────────────────────────────────────────────────────

    [Test]
    public void GetExportDirectory_ReturnsValidPath()
    {
        var mockEnv = new Mock<IWebHostEnvironment>();
        mockEnv.Setup(e => e.ContentRootPath).Returns(@"C:\app");

        var mockResultProvider = new Mock<ISqlBrowserResultProvider>();

        var service = new SqlBrowserExporter(mockEnv.Object, mockResultProvider.Object);
        var path = service.GetExportDirectory();

        Assert.Multiple(() =>
        {
            Assert.That(path, Does.EndWith("Export"));
            Assert.That(path, Does.Contain("App_Data"));
        });
    }

    [Test]
    public void GetExportDirectory_WithContentRootPath_IncludesAppDataAndExport()
    {
        var mockEnv = new Mock<IWebHostEnvironment>();
        mockEnv.Setup(e => e.ContentRootPath).Returns(@"C:\website");

        var mockResultProvider = new Mock<ISqlBrowserResultProvider>();

        var service = new SqlBrowserExporter(mockEnv.Object, mockResultProvider.Object);
        var path = service.GetExportDirectory();

        Assert.Multiple(() =>
        {
            Assert.That(path, Does.Contain("App_Data"));
            Assert.That(path, Does.EndWith("Export"));
        });
    }

    // ────── behavior: Export with no columns ─────────────────────────────────────────────────────

    [Test]
    public async Task Export_WithNoColumns_ReturnsEmptyContent()
    {
        var mockEnv = new Mock<IWebHostEnvironment>();
        var mockResultProvider = new Mock<ISqlBrowserResultProvider>();

        mockResultProvider.Setup(r => r.GetColumnNames()).Returns(new List<string>());
        mockResultProvider.Setup(r => r.GetRowsAsDynamic()).ReturnsAsync(new List<dynamic>());

        var service = new SqlBrowserExporter(mockEnv.Object, mockResultProvider.Object);
        var (content, fileName, contentType) = await service.Export(SqlBrowserExportType.Csv);

        Assert.Multiple(() =>
        {
            Assert.That(content, Is.Empty);
            Assert.That(fileName, Is.Empty);
            Assert.That(contentType, Is.EqualTo("application/octet-stream"));
        });
    }

    // ────── behavior: Export to CSV ──────────────────────────────────────────────────────────────

    [Test]
    public async Task Export_ToCsv_WithDefaultFileName_GeneratesTimestampFileName()
    {
        var mockEnv = new Mock<IWebHostEnvironment>();
        var mockResultProvider = new Mock<ISqlBrowserResultProvider>();

        mockResultProvider.Setup(r => r.GetColumnNames()).Returns(new List<string> { "Id", "Name" });
        mockResultProvider.Setup(r => r.GetRowsAsDynamic()).ReturnsAsync(new List<dynamic>());

        var service = new SqlBrowserExporter(mockEnv.Object, mockResultProvider.Object);
        var (content, fileName, contentType) = await service.Export(SqlBrowserExportType.Csv);

        Assert.Multiple(() =>
        {
            Assert.That(fileName, Does.EndWith(".csv"));
            Assert.That(contentType, Is.EqualTo("text/csv"));
        });
    }

    [Test]
    public async Task Export_ToCsv_WithCustomFileName_UsesCustomName()
    {
        var mockEnv = new Mock<IWebHostEnvironment>();
        var mockResultProvider = new Mock<ISqlBrowserResultProvider>();

        mockResultProvider.Setup(r => r.GetColumnNames()).Returns(new List<string> { "Id", "Name" });
        mockResultProvider.Setup(r => r.GetRowsAsDynamic()).ReturnsAsync(new List<dynamic>());

        var service = new SqlBrowserExporter(mockEnv.Object, mockResultProvider.Object);
        var (content, fileName, contentType) = await service.Export(SqlBrowserExportType.Csv, "MyReport");

        Assert.Multiple(() =>
        {
            Assert.That(fileName, Does.StartWith("MyReport"));
            Assert.That(fileName, Does.EndWith(".csv"));
            Assert.That(contentType, Is.EqualTo("text/csv"));
        });
    }

    [Test]
    public async Task Export_ToCsv_ContainsHeaderRow()
    {
        var mockEnv = new Mock<IWebHostEnvironment>();
        var mockResultProvider = new Mock<ISqlBrowserResultProvider>();

        mockResultProvider.Setup(r => r.GetColumnNames()).Returns(new List<string> { "Id", "Name" });
        mockResultProvider.Setup(r => r.GetRowsAsDynamic()).ReturnsAsync(new List<dynamic>());

        var service = new SqlBrowserExporter(mockEnv.Object, mockResultProvider.Object);
        var (content, _, _) = await service.Export(SqlBrowserExportType.Csv);

        var csv = Encoding.UTF8.GetString(content);
        Assert.That(csv, Does.Contain("Id,Name"));
    }

    // ────── behavior: Export to JSON ─────────────────────────────────────────────────────────────

    [Test]
    public async Task Export_ToJson_ReturnsJsonContentType()
    {
        var mockEnv = new Mock<IWebHostEnvironment>();
        var mockResultProvider = new Mock<ISqlBrowserResultProvider>();

        mockResultProvider.Setup(r => r.GetColumnNames()).Returns(new List<string> { "Id", "Name" });
        mockResultProvider.Setup(r => r.GetRowsAsDynamic()).ReturnsAsync(new List<dynamic>());

        var service = new SqlBrowserExporter(mockEnv.Object, mockResultProvider.Object);
        var (content, fileName, contentType) = await service.Export(SqlBrowserExportType.Json);

        Assert.Multiple(() =>
        {
            Assert.That(fileName, Does.EndWith(".json"));
            Assert.That(contentType, Is.EqualTo("application/json"));
        });
    }

    [Test]
    public async Task Export_ToJson_WithCustomFileName_PreservesFileName()
    {
        var mockEnv = new Mock<IWebHostEnvironment>();
        var mockResultProvider = new Mock<ISqlBrowserResultProvider>();

        mockResultProvider.Setup(r => r.GetColumnNames()).Returns(new List<string> { "Id" });
        mockResultProvider.Setup(r => r.GetRowsAsDynamic()).ReturnsAsync(new List<dynamic>());

        var service = new SqlBrowserExporter(mockEnv.Object, mockResultProvider.Object);
        var (_, fileName, _) = await service.Export(SqlBrowserExportType.Json, "MyData.json");

        Assert.Multiple(() =>
        {
            Assert.That(fileName, Does.StartWith("MyData"));
            Assert.That(fileName, Does.EndWith(".json"));
        });
    }

    // ────── behavior: Export to Excel ────────────────────────────────────────────────────────────

    [Test]
    public async Task Export_ToExcel_ReturnsCorrectExtension()
    {
        var mockEnv = new Mock<IWebHostEnvironment>();
        var mockResultProvider = new Mock<ISqlBrowserResultProvider>();

        mockResultProvider.Setup(r => r.GetColumnNames()).Returns(new List<string> { "Id", "Name" });
        mockResultProvider.Setup(r => r.GetRowsAsDynamic()).ReturnsAsync(new List<dynamic>());

        var service = new SqlBrowserExporter(mockEnv.Object, mockResultProvider.Object);
        var (content, fileName, contentType) = await service.Export(SqlBrowserExportType.Excel);

        Assert.That(fileName, Does.EndWith(".xlsx"));
    }

    [Test]
    public async Task Export_ToExcel_WithCustomFileName_UsesCustomName()
    {
        var mockEnv = new Mock<IWebHostEnvironment>();
        var mockResultProvider = new Mock<ISqlBrowserResultProvider>();

        mockResultProvider.Setup(r => r.GetColumnNames()).Returns(new List<string> { "Id" });
        mockResultProvider.Setup(r => r.GetRowsAsDynamic()).ReturnsAsync(new List<dynamic>());

        var service = new SqlBrowserExporter(mockEnv.Object, mockResultProvider.Object);
        var (_, fileName, _) = await service.Export(SqlBrowserExportType.Excel, "Report.xlsx");

        Assert.Multiple(() =>
        {
            Assert.That(fileName, Does.StartWith("Report"));
            Assert.That(fileName, Does.EndWith(".xlsx"));
        });
    }

    // ────── behavior: Export with unknown type ──────────────────────────────────────────────────

    [Test]
    public async Task Export_WithUnknownType_UsesDefaultExtension()
    {
        var mockEnv = new Mock<IWebHostEnvironment>();
        var mockResultProvider = new Mock<ISqlBrowserResultProvider>();

        mockResultProvider.Setup(r => r.GetColumnNames()).Returns(new List<string> { "Id" });
        mockResultProvider.Setup(r => r.GetRowsAsDynamic()).ReturnsAsync(new List<dynamic>());

        var service = new SqlBrowserExporter(mockEnv.Object, mockResultProvider.Object);
        var (_, fileName, _) = await service.Export(SqlBrowserExportType.None);

        Assert.That(fileName, Does.EndWith(".dat"));
    }
}
