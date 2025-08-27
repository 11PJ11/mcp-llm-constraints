using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using ConstraintMcpServer.Domain;
using ConstraintMcpServer.Infrastructure.Configuration;

namespace ConstraintMcpServer.Tests.Configuration;

/// <summary>
/// Unit tests for configuration validation following TDD discipline.
/// Tests drive the creation of domain types and configuration infrastructure.
/// </summary>
[TestFixture]
public class ConfigurationValidationTests
{
    private string _testConfigPath = null!;
    private IConstraintPackReader _reader = null!;

    [SetUp]
    public void SetUp()
    {
        _testConfigPath = Path.GetTempFileName();
        _reader = new YamlConstraintPackReader();
    }

    [TearDown]
    public void TearDown()
    {
        if (File.Exists(_testConfigPath))
        {
            File.Delete(_testConfigPath);
        }
    }

    [Test]
    public void LoadAsync_ValidYaml_ReturnsConstraintPack()
    {
        // Arrange: Valid YAML configuration
        string validYaml = @"
version: ""0.1.0""
constraints:
  - id: tdd.test-first
    title: ""Write a failing test first""
    priority: 0.92
    phases: [kickoff, red, commit]
    reminders:
      - ""Start with a failing test (RED) before implementation.""
      - ""Let the test drive the API design and behavior.""
";
        File.WriteAllText(_testConfigPath, validYaml);

        // Act: Load configuration
        ConstraintPack result = _reader.LoadAsync(_testConfigPath).Result;

        // Assert: Valid constraint pack created
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.Version, Is.EqualTo("0.1.0"));
            Assert.That(result.Constraints, Has.Count.EqualTo(1));
        });

        Constraint constraint = result.Constraints[0];
        Assert.Multiple(() =>
        {
            Assert.That(constraint.Id.Value, Is.EqualTo("tdd.test-first"));
            Assert.That(constraint.Title, Is.EqualTo("Write a failing test first"));
            Assert.That(constraint.Priority.Value, Is.EqualTo(0.92));
            Assert.That(constraint.Phases, Has.Count.EqualTo(3));
            Assert.That(constraint.Reminders, Has.Count.EqualTo(2));
        });
    }

    [Test]
    public void LoadAsync_InvalidPriorityLow_ThrowsValidationException()
    {
        // Arrange: Priority below valid range (< 0.0)
        string invalidYaml = @"
version: ""0.1.0""
constraints:
  - id: test.invalid-priority
    title: ""Invalid priority""
    priority: -0.1
    phases: [green]
    reminders:
      - ""This should fail validation""
";
        File.WriteAllText(_testConfigPath, invalidYaml);

        // Act & Assert: Should throw validation exception
        ValidationException ex = Assert.ThrowsAsync<ValidationException>(
            async () => await _reader.LoadAsync(_testConfigPath))!;

        Assert.That(ex.Message, Does.Contain("Priority"));
        Assert.That(ex.Message, Does.Contain("0.0"));
        Assert.That(ex.Message, Does.Contain("1.0"));
    }

    [Test]
    public void LoadAsync_InvalidPriorityHigh_ThrowsValidationException()
    {
        // Arrange: Priority above valid range (> 1.0)
        string invalidYaml = @"
version: ""0.1.0""
constraints:
  - id: test.invalid-priority
    title: ""Invalid priority""
    priority: 1.1
    phases: [green]
    reminders:
      - ""This should fail validation""
";
        File.WriteAllText(_testConfigPath, invalidYaml);

        // Act & Assert: Should throw validation exception
        ValidationException ex = Assert.ThrowsAsync<ValidationException>(
            async () => await _reader.LoadAsync(_testConfigPath))!;

        Assert.That(ex.Message, Does.Contain("Priority"));
        Assert.That(ex.Message, Does.Contain("0.0"));
        Assert.That(ex.Message, Does.Contain("1.0"));
    }

    [Test]
    public void LoadAsync_DuplicateConstraintIds_ThrowsValidationException()
    {
        // Arrange: Multiple constraints with same ID
        string invalidYaml = @"
version: ""0.1.0""
constraints:
  - id: duplicate.constraint
    title: ""First constraint""
    priority: 0.8
    phases: [red]
    reminders:
      - ""First reminder""
  - id: duplicate.constraint
    title: ""Second constraint""
    priority: 0.9
    phases: [green]
    reminders:
      - ""Second reminder""
";
        File.WriteAllText(_testConfigPath, invalidYaml);

        // Act & Assert: Should throw validation exception
        ValidationException ex = Assert.ThrowsAsync<ValidationException>(
            async () => await _reader.LoadAsync(_testConfigPath))!;

        Assert.That(ex.Message, Does.Contain("duplicate"));
        Assert.That(ex.Message, Does.Contain("duplicate.constraint"));
    }

    [Test]
    public void LoadAsync_EmptyReminders_ThrowsValidationException()
    {
        // Arrange: Constraint with empty reminders list
        string invalidYaml = @"
version: ""0.1.0""
constraints:
  - id: test.empty-reminders
    title: ""No reminders""
    priority: 0.5
    phases: [refactor]
    reminders: []
";
        File.WriteAllText(_testConfigPath, invalidYaml);

        // Act & Assert: Should throw validation exception
        ValidationException ex = Assert.ThrowsAsync<ValidationException>(
            async () => await _reader.LoadAsync(_testConfigPath))!;

        Assert.That(ex.Message, Does.Contain("reminders"));
        Assert.That(ex.Message, Does.Contain("empty"));
    }

    [Test]
    public void LoadAsync_UnknownPhase_ThrowsValidationException()
    {
        // Arrange: Constraint with unknown phase
        string invalidYaml = @"
version: ""0.1.0""
constraints:
  - id: test.unknown-phase
    title: ""Unknown phase""
    priority: 0.7
    phases: [kickoff, unknown_phase, commit]
    reminders:
      - ""This has an unknown phase""
";
        File.WriteAllText(_testConfigPath, invalidYaml);

        // Act & Assert: Should throw validation exception
        ValidationException ex = Assert.ThrowsAsync<ValidationException>(
            async () => await _reader.LoadAsync(_testConfigPath))!;

        Assert.That(ex.Message, Does.Contain("phase"));
        Assert.That(ex.Message, Does.Contain("unknown_phase"));
    }

    [Test]
    public void LoadAsync_MalformedYaml_ThrowsValidationException()
    {
        // Arrange: Malformed YAML
        string malformedYaml = @"
version: 0.1.0
constraints:
  - id: test.malformed
    title: Malformed YAML
    priority: not_a_number
    phases: [kickoff
    reminders:
      - ""Missing closing bracket""
";
        File.WriteAllText(_testConfigPath, malformedYaml);

        // Act & Assert: Should throw validation exception
        ValidationException ex = Assert.ThrowsAsync<ValidationException>(
            async () => await _reader.LoadAsync(_testConfigPath))!;

        Assert.That(ex.Message, Does.Contain("parse").Or.Contain("malformed").Or.Contain("invalid"));
    }

    [Test]
    public void LoadAsync_FileNotFound_ThrowsFileNotFoundException()
    {
        // Arrange: Non-existent file
        string nonExistentPath = Path.Combine(Path.GetTempPath(), "non-existent-config.yaml");

        // Act & Assert: Should throw file not found exception
        FileNotFoundException ex = Assert.ThrowsAsync<FileNotFoundException>(
            async () => await _reader.LoadAsync(nonExistentPath))!;

        Assert.That(ex.Message, Does.Contain(nonExistentPath));
    }

    [Test]
    public void LoadAsync_ValidMultipleConstraints_ReturnsCorrectConstraintPack()
    {
        // Arrange: Multiple valid constraints
        string validYaml = @"
version: ""0.1.0""
constraints:
  - id: tdd.test-first
    title: ""Write a failing test first""
    priority: 0.92
    phases: [kickoff, red]
    reminders:
      - ""Start with a failing test (RED) before implementation.""
  - id: arch.hex.domain-pure
    title: ""Domain must not depend on Infrastructure""
    priority: 0.88
    phases: [green, refactor]
    reminders:
      - ""Domain layer: pure business logic, no framework dependencies.""
  - id: quality.yagni
    title: ""You Aren't Gonna Need It""
    priority: 0.75
    phases: [commit]
    reminders:
      - ""Implement only what's needed right now.""
";
        File.WriteAllText(_testConfigPath, validYaml);

        // Act: Load configuration
        ConstraintPack result = _reader.LoadAsync(_testConfigPath).Result;

        // Assert: All constraints loaded correctly
        Assert.That(result.Constraints, Has.Count.EqualTo(3));

        Assert.Multiple(() =>
        {
            // Verify constraints are sorted by priority (highest first)
            Assert.That(result.Constraints[0].Priority.Value, Is.EqualTo(0.92));
            Assert.That(result.Constraints[1].Priority.Value, Is.EqualTo(0.88));
            Assert.That(result.Constraints[2].Priority.Value, Is.EqualTo(0.75));
        });

        // Verify all phases are valid
        var allPhases = result.Constraints
            .SelectMany(c => c.Phases)
            .ToList();
        Assert.That(allPhases, Is.All.Not.Null);
    }
}
