using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using ConstraintMcpServer.Application.Conversation;
using ConstraintMcpServer.Domain;
using ConstraintMcpServer.Domain.Common;
using ConstraintMcpServer.Domain.Constraints;
using ConstraintMcpServer.Domain.Conversation;
using ConstraintMcpServer.Domain.Visualization;

namespace ConstraintMcpServer.Application.Visualization;

/// <summary>
/// Interactive editor for conversational constraint generation and modification.
/// Integrates with ConversationalConstraintEngine to provide natural language constraint editing.
/// Implements CUPID properties: Composable, Unix Philosophy, Predictable, Idiomatic, Domain-based.
/// </summary>
public sealed class InteractiveEditor
{
    private readonly ConversationalConstraintEngine _conversationalEngine;
    private readonly Dictionary<Guid, ConversationSession> _activeSessions;
    private readonly Dictionary<Guid, List<EditHistoryEntry>> _sessionHistory;
    private readonly SemaphoreSlim _sessionLock;

    public InteractiveEditor()
    {
        _conversationalEngine = new ConversationalConstraintEngine();
        _activeSessions = new Dictionary<Guid, ConversationSession>();
        _sessionHistory = new Dictionary<Guid, List<EditHistoryEntry>>();
        _sessionLock = new SemaphoreSlim(1, 1);
    }

    /// <summary>
    /// Creates a new interactive editing session for conversational constraint modification.
    /// </summary>
    public async Task<Result<EditingSession, DomainError>> CreateEditingSessionAsync(
        ConstraintLibrary library,
        EditingSessionOptions options)
    {
        await _sessionLock.WaitAsync();
        try
        {
            var sessionId = Guid.NewGuid();
            var editingSession = new EditingSession(sessionId, library, options);

            // Start conversational session for natural language constraint editing
            var conversationResult = await _conversationalEngine.StartConversationAsync();
            if (conversationResult.IsError)
            {
                return Result<EditingSession, DomainError>.Failure(conversationResult.Error);
            }

            // Create conversation ID from the generated session ID
            var conversationIdResult = ConversationId.Create(conversationResult.Value);
            if (conversationIdResult.IsError)
            {
                return Result<EditingSession, DomainError>.Failure(
                    ValidationError.ForField("conversationId", "Failed to create conversation ID"));
            }

            var conversationSession = ConversationSession.StartNew(conversationIdResult.Value);

            _activeSessions[sessionId] = conversationSession;
            _sessionHistory[sessionId] = new List<EditHistoryEntry>();

            return Result<EditingSession, DomainError>.Success(editingSession);
        }
        finally
        {
            _sessionLock.Release();
        }
    }

    /// <summary>
    /// Edits a constraint through conversational interface with natural language processing.
    /// </summary>
    public async Task<Result<EditResult, DomainError>> EditConstraintAsync(
        EditingSession session,
        EditCommand command)
    {
        if (!session.IsActive)
        {
            return Result<EditResult, DomainError>.Failure(
                ValidationError.ForField("session", "Session is not active"));
        }

        await _sessionLock.WaitAsync();
        try
        {
            if (!_activeSessions.TryGetValue(session.SessionId, out var conversationSession))
            {
                return Result<EditResult, DomainError>.Failure(
                    ValidationError.ForField("session", "Conversation session not found"));
            }

            // Find the constraint to edit
            var constraint = session.Library.AtomicConstraints.FirstOrDefault(
                c => c.Id.Equals(command.ConstraintId));

            if (constraint == null)
            {
                return Result<EditResult, DomainError>.Failure(
                    ValidationError.ForField("constraintId", "Constraint not found"));
            }

            // Process the edit through conversational engine
            var naturalLanguageInput = GenerateNaturalLanguageInput(command, constraint);
            var processingResult = await _conversationalEngine.ProcessInputAsync(
                conversationSession.ConversationId, naturalLanguageInput);

            if (processingResult.IsError)
            {
                return Result<EditResult, DomainError>.Failure(processingResult.Error);
            }

            // Apply the modification
            var modifiedFields = new List<string>();
            var newTitle = constraint.Title;
            var newPriority = constraint.Priority;

            switch (command.Field)
            {
                case ConstraintField.Title:
                    newTitle = command.NewValue;
                    modifiedFields.Add("Title");
                    break;
                case ConstraintField.Priority:
                    if (double.TryParse(command.NewValue, out var priority))
                    {
                        newPriority = priority;
                        modifiedFields.Add("Priority");
                    }
                    else
                    {
                        return Result<EditResult, DomainError>.Failure(
                            ValidationError.ForField("newValue", "Invalid priority value"));
                    }
                    break;
            }

            // Record in history
            var historyEntry = new EditHistoryEntry(
                command.ConstraintId,
                command.Field,
                GetOriginalValue(constraint, command.Field),
                command.NewValue,
                DateTime.UtcNow);

            _sessionHistory[session.SessionId].Add(historyEntry);

            // Update conversation session
            _activeSessions[session.SessionId] = conversationSession.RecordInteraction(
                command.ConstraintId.Value);

            var result = new EditResult(command.ConstraintId, modifiedFields, newTitle, newPriority);
            return Result<EditResult, DomainError>.Success(result);
        }
        finally
        {
            _sessionLock.Release();
        }
    }

    /// <summary>
    /// Previews constraint changes before applying them.
    /// </summary>
    public async Task<Result<EditPreview, DomainError>> PreviewEditAsync(
        EditingSession session,
        EditCommand command)
    {
        await Task.CompletedTask;

        var constraint = session.Library.AtomicConstraints.FirstOrDefault(
            c => c.Id.Equals(command.ConstraintId));

        if (constraint == null)
        {
            return Result<EditPreview, DomainError>.Failure(
                ValidationError.ForField("constraintId", "Constraint not found"));
        }

        var originalValue = GetOriginalValue(constraint, command.Field);
        var fieldName = command.Field.ToString();

        var preview = new EditPreview(
            command.ConstraintId,
            originalValue,
            command.NewValue,
            fieldName);

        return Result<EditPreview, DomainError>.Success(preview);
    }

    /// <summary>
    /// Retrieves the editing history for a session.
    /// </summary>
    public async Task<Result<EditingHistory, DomainError>> GetEditingHistoryAsync(EditingSession session)
    {
        await Task.CompletedTask;

        await _sessionLock.WaitAsync();
        try
        {
            if (!_sessionHistory.TryGetValue(session.SessionId, out var history))
            {
                history = new List<EditHistoryEntry>();
            }

            var historyEntries = history.Select(h => new HistoryEntry(h.ConstraintId, h.Field)).ToList();
            var editingHistory = new EditingHistory(historyEntries);

            return Result<EditingHistory, DomainError>.Success(editingHistory);
        }
        finally
        {
            _sessionLock.Release();
        }
    }

    /// <summary>
    /// Undoes the last edit operation in a session.
    /// </summary>
    public async Task<Result<UndoResult, DomainError>> UndoLastEditAsync(EditingSession session)
    {
        await Task.CompletedTask;

        if (!session.Options.EnableUndo)
        {
            return Result<UndoResult, DomainError>.Failure(
                ValidationError.ForField("session", "Undo is not enabled for this session"));
        }

        await _sessionLock.WaitAsync();
        try
        {
            if (!_sessionHistory.TryGetValue(session.SessionId, out var history) || history.Count == 0)
            {
                return Result<UndoResult, DomainError>.Failure(
                    ValidationError.ForField("history", "No edits to undo"));
            }

            var lastEdit = history[^1];
            history.RemoveAt(history.Count - 1);

            var undoneEdit = new HistoryEntry(lastEdit.ConstraintId, lastEdit.Field);
            var undoResult = new UndoResult(undoneEdit, lastEdit.OriginalValue);

            return Result<UndoResult, DomainError>.Success(undoResult);
        }
        finally
        {
            _sessionLock.Release();
        }
    }

    /// <summary>
    /// Closes an editing session and performs cleanup.
    /// </summary>
    public async Task<Result<SessionCloseResult, DomainError>> CloseEditingSessionAsync(EditingSession session)
    {
        await Task.CompletedTask;

        await _sessionLock.WaitAsync();
        try
        {
            var changeCount = 0;
            if (_sessionHistory.TryGetValue(session.SessionId, out var history))
            {
                changeCount = history.Count;
                _sessionHistory.Remove(session.SessionId);
            }

            _activeSessions.Remove(session.SessionId);

            var closeResult = new SessionCloseResult(session.SessionId, true, changeCount);
            return Result<SessionCloseResult, DomainError>.Success(closeResult);
        }
        finally
        {
            _sessionLock.Release();
        }
    }

    private static string GenerateNaturalLanguageInput(EditCommand command, AtomicConstraint constraint)
    {
        return command.Field switch
        {
            ConstraintField.Title => $"Change the title of constraint '{constraint.Title}' to '{command.NewValue}'",
            ConstraintField.Priority => $"Set the priority of constraint '{constraint.Title}' to {command.NewValue}",
            _ => $"Modify {command.Field} of constraint '{constraint.Title}' to '{command.NewValue}'"
        };
    }

    private static string GetOriginalValue(AtomicConstraint constraint, ConstraintField field)
    {
        return field switch
        {
            ConstraintField.Title => constraint.Title,
            ConstraintField.Priority => constraint.Priority.ToString(),
            _ => string.Empty
        };
    }
}

// Interactive Editor result types
public record EditResult(ConstraintId ConstraintId, List<string> ModifiedFields, string NewTitle, double NewPriority);
public record EditPreview(ConstraintId ConstraintId, string OriginalValue, string ProposedValue, string FieldName);
public record EditingHistory(List<HistoryEntry> Changes);
public record HistoryEntry(ConstraintId ConstraintId, ConstraintField Field);
public record UndoResult(HistoryEntry UndoneEdit, string RestoredValue);
public record SessionCloseResult(Guid SessionId, bool WasClosed, int FinalChangeCount);

/// <summary>
/// Internal history entry for tracking changes within editing sessions.
/// </summary>
internal record EditHistoryEntry(
    ConstraintId ConstraintId,
    ConstraintField Field,
    string OriginalValue,
    string NewValue,
    DateTime Timestamp);
