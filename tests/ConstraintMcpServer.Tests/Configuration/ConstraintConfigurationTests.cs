using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ConstraintMcpServer.Infrastructure.Configuration;
using NUnit.Framework;

namespace ConstraintMcpServer.Tests.Configuration;

/// <summary>
/// Unit tests for constraint configuration loading and validation.
/// These tests drive the implementation of YAML constraint pack loading.
/// </summary>
[TestFixture]
public class ConstraintConfigurationTests
{
    private string _tempDirectory = string.Empty;

    [SetUp]
    public void SetUp()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), "constraint-config-tests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }
    }

    [Test]
    public async Task LoadConstraintConfiguration_ValidYaml_ReturnsConstraintPack()
    {
        // Arrange
        string validYaml = """
            version: "0.1.0"
            constraints:
              - id: tdd.test-first
                title: "Write a failing test first"
                priority: 0.92
                phases: [kickoff, red, commit]
                reminders:
                  - "Start with a failing test (RED) before implementation."
                  - "Let the test drive the API design and behavior."
            """;

        string configPath = Path.Combine(_tempDirectory, "valid-constraints.yaml");
        await File.WriteAllTextAsync(configPath, validYaml);

        // Act & Assert - This will fail because IConstraintPackReader doesn't exist yet
        IConstraintPackReader reader = CreateConstraintPackReader();
        ConstraintPack constraintPack = await reader.LoadAsync(configPath);

        // Assert
        Assert.That(constraintPack, Is.Not.Null);
        Assert.That(constraintPack.Version, Is.EqualTo("0.1.0"));
        Assert.That(constraintPack.Constraints, Has.Count.EqualTo(1));

        Constraint constraint = constraintPack.Constraints[0];
        Assert.That(constraint.Id.Value, Is.EqualTo("tdd.test-first"));
        Assert.That(constraint.Title, Is.EqualTo("Write a failing test first"));
        Assert.That(constraint.Priority.Value, Is.EqualTo(0.92));
        Assert.That(constraint.Phases, Is.EquivalentTo(new[] { Phase.Kickoff, Phase.Red, Phase.Commit }));
        Assert.That(constraint.Reminders, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task LoadConstraintConfiguration_InvalidYaml_ThrowsValidationException()
    {
        // Arrange
        string invalidYaml = """
            version: "0.1.0"
            constraints:
              - id: invalid.constraint
                # Missing required fields: title, priority, phases, reminders
            """;

        string configPath = Path.Combine(_tempDirectory, "invalid-constraints.yaml");
        await File.WriteAllTextAsync(configPath, invalidYaml);

        // Act & Assert - This will fail because IConstraintPackReader doesn't exist yet
        IConstraintPackReader reader = CreateConstraintPackReader();

        // Act & Assert
        var ex = Assert.ThrowsAsync<ValidationException>(async () => await reader.LoadAsync(configPath));
        Assert.That(ex.Message, Does.Contain("Title"));
    }

    [Test]
    public void LoadConstraintConfiguration_MissingFile_ThrowsFileNotFoundException()
    {
        // Arrange
        string nonExistentPath = Path.Combine(_tempDirectory, "missing-file.yaml");

        // Act & Assert - This will fail because IConstraintPackReader doesn't exist yet
        IConstraintPackReader reader = CreateConstraintPackReader();

        // Act & Assert
        Assert.ThrowsAsync<FileNotFoundException>(async () => await reader.LoadAsync(nonExistentPath));
    }

    [Test]
    public async Task LoadConstraintConfiguration_InvalidPriority_ThrowsValidationException()
    {
        // Arrange
        string invalidPriorityYaml = """
            version: "0.1.0"
            constraints:
              - id: test.constraint
                title: "Test constraint"
                priority: 1.5  # Invalid: must be between 0 and 1
                phases: [red]
                reminders:
                  - "Test reminder"
            """;

        string configPath = Path.Combine(_tempDirectory, "invalid-priority.yaml");
        await File.WriteAllTextAsync(configPath, invalidPriorityYaml);

        // Act & Assert - This will fail because IConstraintPackReader doesn't exist yet
        IConstraintPackReader reader = CreateConstraintPackReader();

        // Act & Assert
        var ex = Assert.ThrowsAsync<ValidationException>(async () => await reader.LoadAsync(configPath));
        Assert.That(ex.Message, Does.Contain("Priority"));
    }

    [Test]
    public async Task LoadConstraintConfiguration_EmptyReminders_ThrowsValidationException()
    {
        // Arrange
        string emptyRemindersYaml = """
            version: "0.1.0"
            constraints:
              - id: test.constraint
                title: "Test constraint"
                priority: 0.8
                phases: [red]
                reminders: []  # Invalid: must have at least one reminder
            """;

        string configPath = Path.Combine(_tempDirectory, "empty-reminders.yaml");
        await File.WriteAllTextAsync(configPath, emptyRemindersYaml);

        // Act & Assert - This will fail because IConstraintPackReader doesn't exist yet
        IConstraintPackReader reader = CreateConstraintPackReader();

        // Act & Assert
        var ex = Assert.ThrowsAsync<ValidationException>(async () => await reader.LoadAsync(configPath));
        Assert.That(ex.Message, Does.Contain("At least one reminder is required"));
    }

    [Test]
    public async Task ValidateConstraintConfiguration_ValidConfiguration_ReturnsSuccessResult()
    {
        // Arrange
        ConstraintPack constraintPack = CreateValidConstraintPack();

        // Act & Assert - This will fail because IConstraintValidator doesn't exist yet
        IConstraintValidator validator = CreateConstraintValidator();
        ValidationResult result = await validator.ValidateAsync(constraintPack);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsValid, Is.True);
        Assert.That(result.Errors, Is.Empty);
    }

    [Test]
    public async Task ValidateConstraintConfiguration_DuplicateIds_ReturnsValidationError()
    {
        // Arrange
        ConstraintPack constraintPack = CreateConstraintPackWithDuplicateIds();

        // Act & Assert - This will fail because IConstraintValidator doesn't exist yet
        IConstraintValidator validator = CreateConstraintValidator();
        ValidationResult result = await validator.ValidateAsync(constraintPack);

        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Has.Some.Matches<string>(error => error.Contains("duplicate", StringComparison.OrdinalIgnoreCase)));
    }

    // Helper methods that create instances for testing
    private IConstraintPackReader CreateConstraintPackReader()
    {
        return new YamlConstraintPackReader();
    }

    private IConstraintValidator CreateConstraintValidator()
    {
        return new ConstraintPackValidator();
    }

    private ConstraintPack CreateValidConstraintPack()
    {
        var constraint = new Constraint
        {
            Id = new ConstraintId("test.constraint"),
            Title = "Test constraint",
            Priority = new Priority(0.8),
            Phases = new[] { Phase.Red, Phase.Green },
            Reminders = new[] { "Test reminder message" }
        };

        return new ConstraintPack
        {
            Version = "0.1.0",
            Constraints = new[] { constraint }
        };
    }

    private ConstraintPack CreateConstraintPackWithDuplicateIds()
    {
        var constraint1 = new Constraint
        {
            Id = new ConstraintId("duplicate.id"),
            Title = "First constraint",
            Priority = new Priority(0.8),
            Phases = new[] { Phase.Red },
            Reminders = new[] { "First reminder" }
        };

        var constraint2 = new Constraint
        {
            Id = new ConstraintId("duplicate.id"),
            Title = "Second constraint",
            Priority = new Priority(0.7),
            Phases = new[] { Phase.Green },
            Reminders = new[] { "Second reminder" }
        };

        return new ConstraintPack
        {
            Version = "0.1.0",
            Constraints = new[] { constraint1, constraint2 }
        };
    }
}
