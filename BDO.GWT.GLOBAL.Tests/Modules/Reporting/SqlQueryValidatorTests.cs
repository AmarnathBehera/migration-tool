using BDO.GWT.CustomReportingModule.ReportingApp;

namespace BDO.GWT.GLOBAL.Tests.Modules.Reporting;

public class SqlQueryValidatorTests
{
    // ────── structural ──────────────────────────────────────────────────────────────────────────

    [Test]
    public void Service_ImplementsISqlQueryValidator()
    {
        Assert.That(typeof(ISqlQueryValidator).IsAssignableFrom(typeof(SqlQueryValidator)), Is.True);
    }

    [Test]
    public void Service_IsPublicNonAbstractClass()
    {
        Assert.Multiple(() =>
        {
            Assert.That(typeof(SqlQueryValidator).IsPublic, Is.True);
            Assert.That(typeof(SqlQueryValidator).IsAbstract, Is.False);
        });
    }

    [Test]
    public void Service_HasSinglePublicConstructor()
    {
        Assert.That(typeof(SqlQueryValidator).GetConstructors(), Has.Length.EqualTo(1));
    }

    [Test]
    public void Service_Constructor_HasNoParameters()
    {
        Assert.That(typeof(SqlQueryValidator).GetConstructors().Single().GetParameters(), Is.Empty);
    }

    // ────── interface shape ──────────────────────────────────────────────────────────────────────

    [Test]
    public void ISqlQueryValidator_ExposesValidateSqlStatement()
    {
        Assert.That(typeof(ISqlQueryValidator).GetMethod(nameof(ISqlQueryValidator.ValidateSqlStatement)), Is.Not.Null);
    }

    [Test]
    public void ValidateSqlStatement_Parameter_IsString()
    {
        var method = typeof(ISqlQueryValidator).GetMethod(nameof(ISqlQueryValidator.ValidateSqlStatement));
        var parameters = method!.GetParameters();
        Assert.Multiple(() =>
        {
            Assert.That(parameters, Has.Length.EqualTo(1));
            Assert.That(parameters[0].ParameterType, Is.EqualTo(typeof(string)));
        });
    }

    [Test]
    public void ValidateSqlStatement_ReturnType_IsValidationResult()
    {
        var method = typeof(ISqlQueryValidator).GetMethod(nameof(ISqlQueryValidator.ValidateSqlStatement));

        Assert.That(method!.ReturnType, Is.EqualTo(typeof(ValidationResult)));
    }

    // ────── behavior: ValidateSqlStatement ──────────────────────────────────────────────────────

    [Test]
    public void ValidateSqlStatement_WithValidSelectQuery_ReturnsSuccess()
    {
        var service = new SqlQueryValidator();
        var result = service.ValidateSqlStatement("SELECT * FROM Users");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.True);
            Assert.That(result.ErrorMessage, Is.Null);
        });
    }

    [Test]
    public void ValidateSqlStatement_WithValidTopSelectQuery_ReturnsSuccess()
    {
        var service = new SqlQueryValidator();
        var result = service.ValidateSqlStatement("SELECT TOP 2 * FROM Users");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.True);
            Assert.That(result.ErrorMessage, Is.Null);
        });
    }

    [Test]
    public void ValidateSqlStatement_WithDeclareStatement_ReturnsFailure()
    {
        var service = new SqlQueryValidator();
        var result = service.ValidateSqlStatement("DECLARE @id INT SELECT @id");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Query must match the supported SELECT pattern."));
        });
    }

    [Test]
    public void ValidateSqlStatement_WithEmptyString_ReturnsFailure()
    {
        var service = new SqlQueryValidator();
        var result = service.ValidateSqlStatement(string.Empty);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("SQL is empty"));
        });
    }

    [Test]
    public void ValidateSqlStatement_WithWhitespaceOnly_ReturnsFailure()
    {
        var service = new SqlQueryValidator();
        var result = service.ValidateSqlStatement("   \t\n   ");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("SQL is empty"));
        });
    }

    [Test]
    public void ValidateSqlStatement_WithDropStatement_ReturnsFailure()
    {
        var service = new SqlQueryValidator();
        var result = service.ValidateSqlStatement("DROP TABLE Users");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Destructive or multi-statement SQL is not allowed."));
        });
    }

    [Test]
    public void ValidateSqlStatement_WithDropStatement_CaseInsensitive_ReturnsFailure()
    {
        var service = new SqlQueryValidator();
        var result = service.ValidateSqlStatement("drop table Users");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Destructive or multi-statement SQL is not allowed."));
        });
    }

    [Test]
    public void ValidateSqlStatement_WithTruncateStatement_ReturnsFailure()
    {
        var service = new SqlQueryValidator();
        var result = service.ValidateSqlStatement("TRUNCATE TABLE Users");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Destructive or multi-statement SQL is not allowed."));
        });
    }

    [Test]
    public void ValidateSqlStatement_WithMultipleStatements_ReturnsFailure()
    {
        var service = new SqlQueryValidator();
        var result = service.ValidateSqlStatement("SELECT * FROM Users; DELETE FROM Users");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Destructive or multi-statement SQL is not allowed."));
        });
    }

    [Test]
    public void ValidateSqlStatement_WithSemicolonAtEnd_ReturnsFailure()
    {
        var service = new SqlQueryValidator();
        var result = service.ValidateSqlStatement("SELECT * FROM Users;");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Destructive or multi-statement SQL is not allowed."));
        });
    }

    // ────── behavior: edge cases ─────────────────────────────────────────────────────────────────

    [Test]
    public void ValidateSqlStatement_WithLeadingAndTrailingWhitespace_ReturnsSuccess()
    {
        var service = new SqlQueryValidator();
        var result = service.ValidateSqlStatement("   SELECT * FROM Users   \n\t");

        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void ValidateSqlStatement_WithCommentedOutDestructiveStatement_ReturnsSuccess()
    {
        var service = new SqlQueryValidator();
        var result = service.ValidateSqlStatement("-- DROP TABLE Users\nSELECT * FROM Users");

        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void ValidateSqlStatement_WithTableAlias_ReturnsFailure()
    {
        var service = new SqlQueryValidator();
        var result = service.ValidateSqlStatement("SELECT TOP 2 * FROM CMS_User dgd");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Query must match the supported SELECT pattern."));
        });
    }

    [Test]
    public void ValidateSqlStatement_WithComplexJoin_ReturnsFailure()
    {
        var service = new SqlQueryValidator();
        var sql = @"
            SELECT u.Id, u.Name, o.OrderId
            FROM Users u
            INNER JOIN Orders o ON u.Id = o.UserId
            WHERE u.Id > 0";

        var result = service.ValidateSqlStatement(sql);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Query must match the supported SELECT pattern."));
        });
    }
}
