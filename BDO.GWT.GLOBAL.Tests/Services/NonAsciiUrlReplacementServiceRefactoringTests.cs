using BDO.GWT.GLOBAL.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Services;

/// <summary>
/// Additional test cases for refactoring improvements:
/// - ParseRules() helper methods validation
/// - Nested if statements merge
/// - Cognitive complexity reduction
/// - Guarded logging validation
/// - Comment filtering
/// - Duplicate key handling
/// </summary>
public class NonAsciiUrlReplacementServiceRefactoringTests
{
    // ============================================================================
    // PARSE RULES - HELPER METHOD TESTS
    // ============================================================================

    [Test]
    public void ParseRules_FiltersCommentLines_WithHashPrefix()
    {
        // Arrange: Dictionary with hash-prefixed comments
        var dictionary = "# This is a comment;ä=ae;# Another comment;ö=oe";

        // Act
        var rules = NonAsciiUrlReplacementService.ParseRules(dictionary, NullLogger.Instance);

        // Assert: Comments should be filtered out
        Assert.Multiple(() =>
        {
            Assert.That(rules, Has.Count.EqualTo(2));
            Assert.That(rules[0].Search, Is.EqualTo("ä"));
            Assert.That(rules[1].Search, Is.EqualTo("ö"));
        });
    }

    [Test]
    public void ParseRules_FiltersCommentLines_WithDoubleSlashPrefix()
    {
        // Arrange: Dictionary with // comments
        var dictionary = "// This is a comment;ä=ae;// Another comment;ö=oe";

        // Act
        var rules = NonAsciiUrlReplacementService.ParseRules(dictionary, NullLogger.Instance);

        // Assert: Comments should be filtered out
        Assert.Multiple(() =>
        {
            Assert.That(rules, Has.Count.EqualTo(2));
            Assert.That(rules[0].Search, Is.EqualTo("ä"));
            Assert.That(rules[1].Search, Is.EqualTo("ö"));
        });
    }

    [Test]
    public void ParseRules_FiltersEmptyLines()
    {
        // Arrange: Dictionary with empty lines
        var dictionary = ";;ä=ae;;ö=oe;;;";

        // Act
        var rules = NonAsciiUrlReplacementService.ParseRules(dictionary, NullLogger.Instance);

        // Assert: Empty lines filtered, only mappings remain
        Assert.Multiple(() =>
        {
            Assert.That(rules, Has.Count.EqualTo(2));
        });
    }

    [Test]
    public void ParseRules_ValidatesFormatWithEqualsSign()
    {
        // Arrange: Dictionary with various invalid formats
        var dictionary = "no_equals;=only_value;key_=;ä=ae";

        // Act
        var rules = NonAsciiUrlReplacementService.ParseRules(dictionary, NullLogger.Instance);

        // Assert: Invalid formats filtered, only valid entry remains
        Assert.Multiple(() =>
        {
            Assert.That(rules, Has.Count.EqualTo(1));
            Assert.That(rules[0].Search, Is.EqualTo("ä"));
            Assert.That(rules[0].Replacement, Is.EqualTo("ae"));
        });
    }

    [Test]
    public void ParseRules_ValidatesKeyAndValueAreNonEmpty()
    {
        // Arrange: Dictionary with empty key or value
        var dictionary = "=ae;ä=;ö=oe";

        // Act
        var rules = NonAsciiUrlReplacementService.ParseRules(dictionary, NullLogger.Instance);

        // Assert: Only entries with both key and value remain
        Assert.Multiple(() =>
        {
            Assert.That(rules, Has.Count.EqualTo(1));
            Assert.That(rules[0].Search, Is.EqualTo("ö"));
        });
    }

    [Test]
    public void ParseRules_ValidatesKeyAndValueAreNotJustWhitespace()
    {
        // Arrange: Dictionary with whitespace-only key or value
        var dictionary = "   =ae;ä=   ;ö=oe";

        // Act
        var rules = NonAsciiUrlReplacementService.ParseRules(dictionary, NullLogger.Instance);

        // Assert: Only entries where both are non-whitespace remain
        Assert.Multiple(() =>
        {
            Assert.That(rules, Has.Count.EqualTo(1));
            Assert.That(rules[0].Search, Is.EqualTo("ö"));
        });
    }

    [Test]
    public void ParseRules_TrimsMeaninglessWhitespaceAroundKeyAndValue()
    {
        // Arrange: Dictionary with spaces around key/value
        var dictionary = "  ä  =  ae  ;  ö  =  oe  ";

        // Act
        var rules = NonAsciiUrlReplacementService.ParseRules(dictionary, NullLogger.Instance);

        // Assert: Whitespace trimmed but key/value preserved
        Assert.Multiple(() =>
        {
            Assert.That(rules, Has.Count.EqualTo(2));
            Assert.That(rules[0].Search, Is.EqualTo("ä"));
            Assert.That(rules[0].Replacement, Is.EqualTo("ae"));
            Assert.That(rules[1].Search, Is.EqualTo("ö"));
            Assert.That(rules[1].Replacement, Is.EqualTo("oe"));
        });
    }

    // ============================================================================
    // PARSE RULES - DUPLICATE HANDLING
    // ============================================================================

    [Test]
    public void ParseRules_HandlesDuplicateKeys_LastValueWins()
    {
        // Arrange: Dictionary with duplicate keys
        var dictionary = "ä=first;ä=second;ä=third";

        // Act
        var rules = NonAsciiUrlReplacementService.ParseRules(dictionary, NullLogger.Instance);

        // Assert: All duplicates included in rules (sorted), so longest key+last value should apply
        Assert.Multiple(() =>
        {
            Assert.That(rules, Has.Count.EqualTo(1));
            Assert.That(rules[0].Search, Is.EqualTo("ä"));
            Assert.That(rules[0].Replacement, Is.EqualTo("third"));
        });
    }

    [Test]
    public void ParseRules_LogsDuplicateKeyWarning_WhenDuplicatesDetected()
    {
        // Arrange: Mock logger with IsEnabled returning true
        var mockLogger = new Mock<ILogger>();
        mockLogger
            .Setup(x => x.IsEnabled(LogLevel.Warning))
            .Returns(true);  // Logger is enabled

        var dictionary = "ä=ae;ä=aa";  // Duplicate

        // Act
        var rules = NonAsciiUrlReplacementService.ParseRules(dictionary, mockLogger.Object);

        // Assert: Warning logged for duplicate
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Duplicate")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "Should log warning for duplicate key");
    }

    [Test]
    public void ParseRules_LogsInvalidEntryWarning_WhenFormatInvalid()
    {
        // Arrange: Mock logger with IsEnabled returning true
        var mockLogger = new Mock<ILogger>();
        mockLogger
            .Setup(x => x.IsEnabled(LogLevel.Warning))
            .Returns(true);  // Logger is enabled

        var dictionary = "invalid_format;ä=ae";  // First entry is invalid

        // Act
        var rules = NonAsciiUrlReplacementService.ParseRules(dictionary, mockLogger.Object);

        // Assert: Warning logged for invalid entry
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Ignored invalid")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "Should log warning for invalid format");
    }

    // ============================================================================
    // PARSE RULES - SORTING
    // ============================================================================

    [Test]
    public void ParseRules_SortsByKeyLengthDescending_ThenAlphabetically()
    {
        // Arrange: Dictionary with keys of varying lengths
        var dictionary = "a=A;ab=AB;abc=ABC;abd=ABD";

        // Act
        var rules = NonAsciiUrlReplacementService.ParseRules(dictionary, NullLogger.Instance);

        // Assert: Sorted by length descending, then alphabetically
        Assert.Multiple(() =>
        {
            Assert.That(rules, Has.Count.EqualTo(4));
            Assert.That(rules[0].Search, Is.EqualTo("abc"));
            Assert.That(rules[1].Search, Is.EqualTo("abd"));
            Assert.That(rules[2].Search, Is.EqualTo("ab"));
            Assert.That(rules[3].Search, Is.EqualTo("a"));
        });
    }

    [Test]
    public void ParseRules_PrioritizesLongerKeysInSorting()
    {
        // Arrange: Longer keys should be processed first in replacements
        var dictionary = "ä=ae;äe=aeX;äeä=aeXae";

        // Act
        var rules = NonAsciiUrlReplacementService.ParseRules(dictionary, NullLogger.Instance);

        // Assert: Longest keys first
        Assert.Multiple(() =>
        {
            Assert.That(rules, Has.Count.EqualTo(3));
            Assert.That(rules[0].Search, Is.EqualTo("äeä"));
            Assert.That(rules[1].Search, Is.EqualTo("äe"));
            Assert.That(rules[2].Search, Is.EqualTo("ä"));
        });
    }

    // ============================================================================
    // PARSE RULES - EMPTY INPUT HANDLING
    // ============================================================================

    [Test]
    public void ParseRules_ReturnsEmptyArray_WhenInputIsNull()
    {
        var rules = NonAsciiUrlReplacementService.ParseRules(null, NullLogger.Instance);

        Assert.Multiple(() =>
        {
            Assert.That(rules, Has.Count.EqualTo(0));
        });
    }

    [Test]
    public void ParseRules_ReturnsEmptyArray_WhenInputIsEmpty()
    {
        var rules = NonAsciiUrlReplacementService.ParseRules("", NullLogger.Instance);

        Assert.Multiple(() =>
        {
            Assert.That(rules, Has.Count.EqualTo(0));
        });
    }

    [Test]
    public void ParseRules_ReturnsEmptyArray_WhenInputIsWhitespace()
    {
        var rules = NonAsciiUrlReplacementService.ParseRules("   \n\n  ", NullLogger.Instance);

        Assert.Multiple(() =>
        {
            Assert.That(rules, Has.Count.EqualTo(0));
        });
    }

    [Test]
    public void ParseRules_ReturnsEmptyArray_WhenAllLinesAreComments()
    {
        var dictionary = "# Comment;// Comment;# Another";

        var rules = NonAsciiUrlReplacementService.ParseRules(dictionary, NullLogger.Instance);

        Assert.Multiple(() =>
        {
            Assert.That(rules, Has.Count.EqualTo(0));
        });
    }

    // ============================================================================
    // APPLY RULES - MULTI-RULE APPLICATION
    // ============================================================================

    [Test]
    public void ApplyRules_AppliesMultipleRulesInOrder()
    {
        // Arrange: Multiple replacement rules
        var rules = new[]
        {
            new NonAsciiUrlReplacementRule("ä", "ae"),
            new NonAsciiUrlReplacementRule("ö", "oe")
        };

        // Act
        var result = NonAsciiUrlReplacementService.ApplyRules("äö", rules);

        // Assert: All rules applied
        Assert.Multiple(() =>
        {
            Assert.That(result.Value, Is.EqualTo("aeoe"));
            Assert.That(result.ReplacementCount, Is.EqualTo(2));
        });
    }

    [Test]
    public void ApplyRules_CountsReplacementsAcrossAllRules()
    {
        // Arrange: Multiple occurrences and multiple rules
        var rules = new[]
        {
            new NonAsciiUrlReplacementRule("ä", "ae"),
            new NonAsciiUrlReplacementRule("ö", "oe")
        };

        // Act
        var result = NonAsciiUrlReplacementService.ApplyRules("äöä", rules);

        // Assert: Count aggregated correctly
        Assert.Multiple(() =>
        {
            Assert.That(result.ReplacementCount, Is.EqualTo(3));
        });
    }

    [Test]
    public void ApplyRules_ReturnsZeroReplacements_WhenNoMatches()
    {
        // Arrange
        var rules = new[] { new NonAsciiUrlReplacementRule("xyz", "abc") };

        // Act
        var result = NonAsciiUrlReplacementService.ApplyRules("no-match", rules);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Value, Is.EqualTo("no-match"));
            Assert.That(result.ReplacementCount, Is.EqualTo(0));
        });
    }

    // ============================================================================
    // REPLACE ALL - SUBSTRING HANDLING
    // ============================================================================

    [Test]
    public void ApplyRules_ReplacesAllOccurrences_NotJustFirst()
    {
        // Arrange: Multiple occurrences of same pattern
        var rules = new[] { new NonAsciiUrlReplacementRule("a", "X") };

        // Act
        var result = NonAsciiUrlReplacementService.ApplyRules("banana", rules);

        // Assert: All 3 'a' replaced
        Assert.Multiple(() =>
        {
            Assert.That(result.Value, Is.EqualTo("bXnXnX"));
            Assert.That(result.ReplacementCount, Is.EqualTo(3));
        });
    }

    [Test]
    public void ApplyRules_HandleConsecutiveReplacement_CorrectlyWithoutDoubleProcessing()
    {
        // Arrange: Adjacent matches
        var rules = new[] { new NonAsciiUrlReplacementRule("aa", "X") };

        // Act
        var result = NonAsciiUrlReplacementService.ApplyRules("aaaa", rules);

        // Assert: Should replace non-overlapping occurrences
        Assert.Multiple(() =>
        {
            Assert.That(result.Value, Is.EqualTo("XX"));
            Assert.That(result.ReplacementCount, Is.EqualTo(2));
        });
    }

    [Test]
    public void ApplyRules_HandlesEmptyReplacement()
    {
        // Arrange: Empty replacement string
        var rules = new[] { new NonAsciiUrlReplacementRule("a", "") };

        // Act
        var result = NonAsciiUrlReplacementService.ApplyRules("banana", rules);

        // Assert: Character removed
        Assert.Multiple(() =>
        {
            Assert.That(result.Value, Is.EqualTo("bnn"));
            Assert.That(result.ReplacementCount, Is.EqualTo(3));
        });
    }

    // ============================================================================
    // GUARDED LOGGING - NO EXPENSIVE EVALUATION
    // ============================================================================

    [Test]
    public void ParseRules_DoesNotCallLogger_WhenLoggerIsNull()
    {
        // Arrange: Null logger should not cause NullReferenceException
        var dictionary = "invalid-entry;ä=ae";

        // Act & Assert: Should not throw
        Assert.DoesNotThrow(() =>
        {
            var rules = NonAsciiUrlReplacementService.ParseRules(dictionary, null);
            Assert.Multiple(() =>
            {
                Assert.That(rules, Has.Count.EqualTo(1));
            });
        });
    }

    [Test]
    public void ParseRules_DoesNotLogWarning_WhenWarningNotEnabled()
    {
        // Arrange: Mock logger that returns false for Warning level
        var mockLogger = new Mock<ILogger>();
        mockLogger
            .Setup(x => x.IsEnabled(LogLevel.Warning))
            .Returns(false);

        var dictionary = "invalid-entry;ä=ae";

        // Act
        var rules = NonAsciiUrlReplacementService.ParseRules(dictionary, mockLogger.Object);

        // Assert: No Log calls should occur (guard prevented evaluation)
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never,
            "Should not log when Warning level disabled");
    }
}
