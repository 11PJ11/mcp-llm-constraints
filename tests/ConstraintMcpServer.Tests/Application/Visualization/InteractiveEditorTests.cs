using System;
using System.Threading.Tasks;
using NUnit.Framework;
using ConstraintMcpServer.Application.Visualization;
using ConstraintMcpServer.Domain;
using ConstraintMcpServer.Domain.Constraints;
using ConstraintMcpServer.Domain.Visualization;

namespace ConstraintMcpServer.Tests.Application.Visualization;

/// <summary>
/// Unit tests for InteractiveEditor application service.
/// Drives implementation through TDD Red-Green-Refactor cycles.
/// Tests focus on live constraint editing and interactive modification capabilities.
/// </summary>
[TestFixture]
[Category("Unit")]
[Category("Application")]
[Category("Interactive")]
public sealed class InteractiveEditorTests
{
    private InteractiveEditor _editor = null!;
    private ConstraintLibrary _testLibrary = null!;

    [SetUp]
    public void Setup()
    {
        _editor = new InteractiveEditor();
        _testLibrary = CreateTestConstraintLibrary();
    }

    [TearDown]
    public void TearDown()
    {
        // No cleanup needed for stateless editor
    }

    /// <summary>
    /// RED: Test should fail initially - should create interactive editing session
    /// </summary>
    [Test]
    public async Task CreateEditingSessionAsync_WhenGivenConstraintLibrary_ShouldCreateSession()
    {
        // Given
        var sessionOptions = new EditingSessionOptions();

        // When
        var result = await _editor.CreateEditingSessionAsync(_testLibrary, sessionOptions);

        // Then
        Assert.That(result.IsSuccess, Is.True, "Should successfully create editing session");
        Assert.That(result.Value, Is.Not.Null, "Should return editing session");
        Assert.That(result.Value.SessionId, Is.Not.EqualTo(Guid.Empty), "Should have valid session ID");
        Assert.That(result.Value.IsActive, Is.True, "Session should be active");
        Assert.That(result.Value.Library, Is.EqualTo(_testLibrary), "Should reference provided library");
    }

    /// <summary>
    /// RED: Test should fail initially - should edit constraint title interactively
    /// </summary>
    [Test]
    public async Task EditConstraintAsync_WhenModifyingTitle_ShouldUpdateConstraint()
    {
        // Given
        var session = await CreateActiveSession();
        var constraintId = new ConstraintId("test.constraint.1");
        var editCommand = new EditCommand
        {
            ConstraintId = constraintId,
            Field = ConstraintField.Title,
            NewValue = "Updated Test Constraint"
        };

        // When
        var result = await _editor.EditConstraintAsync(session, editCommand);

        // Then
        Assert.That(result.IsSuccess, Is.True, "Should successfully edit constraint");
        Assert.That(result.Value.ConstraintId, Is.EqualTo(constraintId), "Should return edited constraint ID");
        Assert.That(result.Value.ModifiedFields, Does.Contain("Title"), "Should track title modification");
        Assert.That(result.Value.NewTitle, Is.EqualTo("Updated Test Constraint"), "Should have updated title");
    }

    /// <summary>
    /// RED: Test should fail initially - should edit constraint priority interactively
    /// </summary>
    [Test]
    public async Task EditConstraintAsync_WhenModifyingPriority_ShouldUpdatePriority()
    {
        // Given
        var session = await CreateActiveSession();
        var constraintId = new ConstraintId("test.constraint.1");
        var editCommand = new EditCommand
        {
            ConstraintId = constraintId,
            Field = ConstraintField.Priority,
            NewValue = "0.95"
        };

        // When
        var result = await _editor.EditConstraintAsync(session, editCommand);

        // Then
        Assert.That(result.IsSuccess, Is.True, "Should successfully edit constraint priority");
        Assert.That(result.Value.ConstraintId, Is.EqualTo(constraintId), "Should return edited constraint ID");
        Assert.That(result.Value.ModifiedFields, Does.Contain("Priority"), "Should track priority modification");
        Assert.That(result.Value.NewPriority, Is.EqualTo(0.95), "Should have updated priority");
    }

    /// <summary>
    /// RED: Test should fail initially - should validate edit commands
    /// </summary>
    [Test]
    public async Task EditConstraintAsync_WhenGivenInvalidCommand_ShouldReturnValidationError()
    {
        // Given
        var session = await CreateActiveSession();
        var invalidCommand = new EditCommand
        {
            ConstraintId = new ConstraintId("non.existent.constraint"),
            Field = ConstraintField.Title,
            NewValue = "Some Title"
        };

        // When
        var result = await _editor.EditConstraintAsync(session, invalidCommand);

        // Then
        Assert.That(result.IsError, Is.True, "Should return error for invalid command");
        Assert.That(result.Error.Message, Does.Contain("not found"), "Should explain constraint not found");
    }

    /// <summary>
    /// RED: Test should fail initially - should preview changes before applying
    /// </summary>
    [Test]
    public async Task PreviewEditAsync_WhenGivenEditCommand_ShouldShowPreview()
    {
        // Given
        var session = await CreateActiveSession();
        var constraintId = new ConstraintId("test.constraint.1");
        var editCommand = new EditCommand
        {
            ConstraintId = constraintId,
            Field = ConstraintField.Title,
            NewValue = "New Title Preview"
        };

        // When
        var result = await _editor.PreviewEditAsync(session, editCommand);

        // Then
        Assert.That(result.IsSuccess, Is.True, "Should successfully generate preview");
        Assert.That(result.Value.ConstraintId, Is.EqualTo(constraintId), "Should preview correct constraint");
        Assert.That(result.Value.OriginalValue, Is.Not.EqualTo("New Title Preview"), "Should show original value");
        Assert.That(result.Value.ProposedValue, Is.EqualTo("New Title Preview"), "Should show proposed value");
        Assert.That(result.Value.FieldName, Is.EqualTo("Title"), "Should identify field being changed");
    }

    /// <summary>
    /// RED: Test should fail initially - should track editing history
    /// </summary>
    [Test]
    public async Task GetEditingHistoryAsync_WhenChangesAreMade_ShouldTrackHistory()
    {
        // Given
        var session = await CreateActiveSession();
        var editCommand = new EditCommand
        {
            ConstraintId = new ConstraintId("test.constraint.1"),
            Field = ConstraintField.Title,
            NewValue = "Modified Title"
        };

        // When
        await _editor.EditConstraintAsync(session, editCommand);
        var historyResult = await _editor.GetEditingHistoryAsync(session);

        // Then
        Assert.That(historyResult.IsSuccess, Is.True, "Should successfully retrieve history");
        Assert.That(historyResult.Value.Changes, Is.Not.Empty, "Should have recorded changes");
        Assert.That(historyResult.Value.Changes.Count, Is.EqualTo(1), "Should have one change recorded");
        Assert.That(historyResult.Value.Changes[0].ConstraintId.Value, Is.EqualTo("test.constraint.1"), "Should track correct constraint");
        Assert.That(historyResult.Value.Changes[0].Field, Is.EqualTo(ConstraintField.Title), "Should track correct field");
    }

    /// <summary>
    /// RED: Test should fail initially - should support undo operations
    /// </summary>
    [Test]
    public async Task UndoLastEditAsync_WhenChangesWereMade_ShouldRevertChange()
    {
        // Given
        var session = await CreateActiveSession();
        var originalConstraint = session.Library.AtomicConstraints.First(c => c.Id.Value == "test.constraint.1");
        var originalTitle = originalConstraint.Title;

        var editCommand = new EditCommand
        {
            ConstraintId = new ConstraintId("test.constraint.1"),
            Field = ConstraintField.Title,
            NewValue = "Temporary Title"
        };

        await _editor.EditConstraintAsync(session, editCommand);

        // When
        var undoResult = await _editor.UndoLastEditAsync(session);

        // Then
        Assert.That(undoResult.IsSuccess, Is.True, "Should successfully undo last edit");
        Assert.That(undoResult.Value.UndoneEdit.ConstraintId.Value, Is.EqualTo("test.constraint.1"), "Should undo correct constraint");
        Assert.That(undoResult.Value.UndoneEdit.Field, Is.EqualTo(ConstraintField.Title), "Should undo correct field");
        Assert.That(undoResult.Value.RestoredValue, Is.EqualTo(originalTitle), "Should restore original value");
    }

    /// <summary>
    /// RED: Test should fail initially - should validate editing session state
    /// </summary>
    [Test]
    public async Task EditConstraintAsync_WhenSessionNotActive_ShouldReturnError()
    {
        // Given
        var inactiveSession = new EditingSession(Guid.NewGuid(), _testLibrary, EditingSessionOptions.Default)
        {
            IsActive = false
        };

        var editCommand = new EditCommand
        {
            ConstraintId = new ConstraintId("test.constraint.1"),
            Field = ConstraintField.Title,
            NewValue = "Some Title"
        };

        // When
        var result = await _editor.EditConstraintAsync(inactiveSession, editCommand);

        // Then
        Assert.That(result.IsError, Is.True, "Should return error for inactive session");
        Assert.That(result.Error.Message, Does.Contain("not active"), "Should explain session not active");
    }

    /// <summary>
    /// RED: Test should fail initially - should close editing session properly
    /// </summary>
    [Test]
    public async Task CloseEditingSessionAsync_WhenSessionActive_ShouldCloseSession()
    {
        // Given
        var session = await CreateActiveSession();

        // When
        var result = await _editor.CloseEditingSessionAsync(session);

        // Then
        Assert.That(result.IsSuccess, Is.True, "Should successfully close session");
        Assert.That(result.Value.SessionId, Is.EqualTo(session.SessionId), "Should close correct session");
        Assert.That(result.Value.WasClosed, Is.True, "Should indicate session was closed");
        Assert.That(result.Value.FinalChangeCount, Is.GreaterThanOrEqualTo(0), "Should report final change count");
    }

    /// <summary>
    /// RED: Test should fail initially - should handle concurrent editing safely
    /// </summary>
    [Test]
    public async Task EditConstraintAsync_WhenConcurrentEdits_ShouldHandleGracefully()
    {
        // Given
        var session = await CreateActiveSession();
        var constraintId = new ConstraintId("test.constraint.1");

        var editCommand1 = new EditCommand
        {
            ConstraintId = constraintId,
            Field = ConstraintField.Title,
            NewValue = "First Edit"
        };

        var editCommand2 = new EditCommand
        {
            ConstraintId = constraintId,
            Field = ConstraintField.Priority,
            NewValue = "0.99"
        };

        // When
        var task1 = _editor.EditConstraintAsync(session, editCommand1);
        var task2 = _editor.EditConstraintAsync(session, editCommand2);

        await Task.WhenAll(task1, task2);

        // Then
        Assert.That(task1.Result.IsSuccess, Is.True, "First edit should succeed");
        Assert.That(task2.Result.IsSuccess, Is.True, "Second edit should succeed");

        var historyResult = await _editor.GetEditingHistoryAsync(session);
        Assert.That(historyResult.Value.Changes.Count, Is.EqualTo(2), "Should record both edits");
    }

    // Helper method to create active editing session
    private async Task<EditingSession> CreateActiveSession()
    {
        var result = await _editor.CreateEditingSessionAsync(_testLibrary, EditingSessionOptions.Default);
        Assert.That(result.IsSuccess, Is.True, "Should create session successfully");
        return result.Value;
    }

    private static ConstraintLibrary CreateTestConstraintLibrary()
    {
        var library = new ConstraintLibrary("1.0.0", "Interactive Test Library");

        // Add test constraints
        library.AddAtomicConstraint(CreateTestAtomicConstraint("test.constraint.1", "Test Constraint 1", 0.8));
        library.AddAtomicConstraint(CreateTestAtomicConstraint("test.constraint.2", "Test Constraint 2", 0.6));

        return library;
    }

    private static AtomicConstraint CreateTestAtomicConstraint(string id, string title, double priority)
    {
        var constraintId = new ConstraintId(id);
        var keywords = System.Collections.Immutable.ImmutableList.Create("test", "interactive");
        var filePatterns = System.Collections.Immutable.ImmutableList.Create("*.cs");
        var contextPatterns = System.Collections.Immutable.ImmutableList.Create("testing");
        var triggers = new TriggerConfiguration(keywords, filePatterns, contextPatterns);
        var reminders = System.Collections.Immutable.ImmutableList.Create($"Remember: {title}");

        return new AtomicConstraint(constraintId, title, priority, triggers, reminders);
    }
}
