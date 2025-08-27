using System.Collections.Generic;
using ConstraintMcpServer.Domain.Constraints;

namespace ConstraintMcpServer.Application.Selection;

/// <summary>
/// Analyzes user input and development session to extract trigger context.
/// Bridges MCP tool calls to business domain context.
/// </summary>
public interface IContextAnalyzer
{
    /// <summary>
    /// Extract development context from MCP tool calls.
    /// Analyzes method name, parameters, and session state to determine trigger context.
    /// </summary>
    /// <param name="methodName">MCP method being called</param>
    /// <param name="parameters">Method parameters containing contextual information</param>
    /// <param name="sessionId">Session identifier for context tracking</param>
    /// <returns>Trigger context extracted from tool call</returns>
    TriggerContext AnalyzeToolCallContext(string methodName, object[] parameters, string sessionId);

    /// <summary>
    /// Extract keywords and patterns from user input.
    /// Processes natural language input to identify development intent and context.
    /// </summary>
    /// <param name="userInput">Natural language input from user</param>
    /// <param name="sessionId">Session identifier for context tracking</param>
    /// <returns>Trigger context extracted from user input</returns>
    TriggerContext AnalyzeUserInput(string userInput, string sessionId);

    /// <summary>
    /// Detect development activity type based on keywords and file patterns.
    /// Classifies the type of development work being performed.
    /// </summary>
    /// <param name="keywords">Keywords extracted from context</param>
    /// <param name="filePath">File path indicating context</param>
    /// <returns>Context type classification (e.g., "feature_development", "refactoring", "testing")</returns>
    string DetectContextType(IEnumerable<string> keywords, string filePath);
}
