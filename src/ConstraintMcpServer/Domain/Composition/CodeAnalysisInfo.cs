using System;
using System.Collections.Generic;

namespace ConstraintMcpServer.Domain.Composition;

/// <summary>
/// Contains code analysis information for architectural composition decisions.
/// Used by LayeredComposition to detect layer dependency violations.
/// </summary>
public sealed record CodeAnalysisInfo
{
    /// <summary>
    /// Dependencies detected in the current codebase.
    /// </summary>
    public IReadOnlyList<DependencyInfo> Dependencies { get; init; } = Array.Empty<DependencyInfo>();

    /// <summary>
    /// Current file being worked on, if any.
    /// </summary>
    public FileInfo? CurrentFile { get; init; }

    /// <summary>
    /// Project structure information.
    /// </summary>
    public ProjectStructureInfo? ProjectStructure { get; init; }

    /// <summary>
    /// Creates code analysis info with dependencies.
    /// </summary>
    public static CodeAnalysisInfo WithDependencies(IEnumerable<DependencyInfo> dependencies)
    {
        return new CodeAnalysisInfo { Dependencies = dependencies.ToArray() };
    }

    /// <summary>
    /// Creates code analysis info with current file.
    /// </summary>
    public static CodeAnalysisInfo WithCurrentFile(FileInfo currentFile)
    {
        return new CodeAnalysisInfo { CurrentFile = currentFile };
    }

    /// <summary>
    /// Creates empty code analysis info.
    /// </summary>
    public static CodeAnalysisInfo Empty => new();
}

/// <summary>
/// Represents a dependency between code elements.
/// </summary>
public sealed record DependencyInfo(
    string Source,
    string Target,
    DependencyType Type = DependencyType.Reference)
{
    /// <summary>
    /// String representation for debugging.
    /// </summary>
    public override string ToString() => $"{Source} → {Target} ({Type})";
}

/// <summary>
/// Represents information about a file being worked on.
/// </summary>
public sealed record FileInfo(
    string FilePath,
    string? Namespace = null,
    string? ClassName = null)
{
    /// <summary>
    /// String representation for debugging.
    /// </summary>
    public override string ToString() => $"{FilePath} ({Namespace})";
}

/// <summary>
/// Contains project structure information.
/// </summary>
public sealed record ProjectStructureInfo(
    IReadOnlyList<string> LayerDirectories)
{
    /// <summary>
    /// Creates project structure info with layer directories.
    /// </summary>
    public static ProjectStructureInfo WithLayers(params string[] directories)
    {
        return new ProjectStructureInfo(directories);
    }
}

/// <summary>
/// Types of dependencies between code elements.
/// </summary>
public enum DependencyType
{
    /// <summary>
    /// Direct reference (using, import, etc.).
    /// </summary>
    Reference,

    /// <summary>
    /// Inheritance relationship.
    /// </summary>
    Inheritance,

    /// <summary>
    /// Interface implementation.
    /// </summary>
    Implementation,

    /// <summary>
    /// Method call or property access.
    /// </summary>
    Usage
}
