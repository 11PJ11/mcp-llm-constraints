using System;
using System.Collections.Generic;
using System.Linq;

namespace ConstraintMcpServer.Domain.Common;

/// <summary>
/// Represents a user-defined hierarchy for hierarchical constraint composition.
/// This class allows users to define their own hierarchical structures with custom
/// levels, priorities, and progression rules for any methodology or practice.
/// </summary>
public sealed class UserDefinedHierarchy : IEquatable<UserDefinedHierarchy>
{
    /// <summary>
    /// Gets the user-defined name of this hierarchy.
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// Gets the user-defined description of this hierarchy.
    /// </summary>
    public string? Description { get; }
    
    /// <summary>
    /// Gets the user-defined hierarchy levels with their descriptions.
    /// Key is the level number, Value is the level description.
    /// </summary>
    public IReadOnlyDictionary<int, string> Levels { get; }
    
    /// <summary>
    /// Gets additional user-defined metadata for this hierarchy.
    /// </summary>
    public IReadOnlyDictionary<string, object> Metadata { get; }
    
    /// <summary>
    /// Initializes a new instance of UserDefinedHierarchy.
    /// </summary>
    /// <param name="name">The user-defined hierarchy name.</param>
    /// <param name="levels">The user-defined hierarchy levels.</param>
    /// <param name="description">Optional user-defined description.</param>
    /// <param name="metadata">Optional user-defined metadata.</param>
    /// <exception cref="ArgumentException">Thrown when name is invalid or levels are empty.</exception>
    public UserDefinedHierarchy(
        string name,
        IReadOnlyDictionary<int, string> levels,
        string? description = null,
        IReadOnlyDictionary<string, object>? metadata = null)
    {
        ValidationHelpers.ValidateAll(
            () => ValidationHelpers.RequireNonEmptyString(name, nameof(name), "Hierarchy name"),
            () => ValidationHelpers.RequireNonEmptyCollection(levels, nameof(levels), "Levels collection")
        );
        
        Name = name.Trim();
        Description = description?.Trim();
        Levels = levels;
        Metadata = metadata ?? new Dictionary<string, object>();
    }
    
    /// <summary>
    /// Initializes a new instance of UserDefinedHierarchy using parameter object.
    /// </summary>
    /// <param name="parameters">The hierarchy parameters.</param>
    /// <exception cref="ArgumentNullException">Thrown when parameters is null.</exception>
    public UserDefinedHierarchy(HierarchyParameters parameters)
    {
        ValidationHelpers.RequireNotNull(parameters, nameof(parameters), "Hierarchy parameters");
        
        Name = parameters.Name;
        Description = parameters.Description;
        Levels = parameters.Levels;
        Metadata = parameters.Metadata;
    }
    
    /// <summary>
    /// Creates a UserDefinedHierarchy from simple user configuration.
    /// </summary>
    /// <param name="name">The user-defined hierarchy name.</param>
    /// <param name="levelDefinitions">User-defined level definitions.</param>
    /// <returns>A new UserDefinedHierarchy instance.</returns>
    public static UserDefinedHierarchy FromUserConfiguration(
        string name,
        IEnumerable<(int level, string description)> levelDefinitions)
    {
        var levels = levelDefinitions.ToDictionary(
            def => def.level,
            def => def.description);
            
        return new UserDefinedHierarchy(name, levels);
    }
    
    /// <summary>
    /// Creates a UserDefinedHierarchy with sequential levels.
    /// </summary>
    /// <param name="name">The user-defined hierarchy name.</param>
    /// <param name="levelDescriptions">Ordered level descriptions (index 0 = level 0).</param>
    /// <returns>A new UserDefinedHierarchy instance with sequential levels.</returns>
    public static UserDefinedHierarchy WithSequentialLevels(string name, params string[] levelDescriptions)
    {
        var levels = levelDescriptions
            .Select((description, index) => new { Level = index, Description = description })
            .ToDictionary(x => x.Level, x => x.Description);
            
        return new UserDefinedHierarchy(name, levels);
    }
    
    /// <summary>
    /// Checks if the specified level exists in this hierarchy.
    /// </summary>
    /// <param name="level">The level to check.</param>
    /// <returns>True if the level exists in this hierarchy.</returns>
    public bool HasLevel(int level)
    {
        return Levels.ContainsKey(level);
    }
    
    /// <summary>
    /// Gets the user-defined description for a hierarchy level.
    /// </summary>
    /// <param name="level">The hierarchy level.</param>
    /// <returns>The user-defined description for the level.</returns>
    public string GetLevelDescription(int level)
    {
        return Levels.ContainsKey(level) 
            ? Levels[level] 
            : $"Level {level}";
    }
    
    /// <summary>
    /// Gets all hierarchy levels ordered by priority (lowest number first).
    /// </summary>
    /// <returns>Hierarchy levels in priority order.</returns>
    public IReadOnlyList<int> GetOrderedLevels()
    {
        return Levels.Keys.OrderBy(level => level).ToList();
    }
    
    /// <summary>
    /// Gets the highest priority (lowest number) level in this hierarchy.
    /// </summary>
    /// <returns>The highest priority level.</returns>
    public int GetHighestPriorityLevel()
    {
        return Levels.Keys.Min();
    }
    
    /// <summary>
    /// Gets the lowest priority (highest number) level in this hierarchy.
    /// </summary>
    /// <returns>The lowest priority level.</returns>
    public int GetLowestPriorityLevel()
    {
        return Levels.Keys.Max();
    }
    
    /// <summary>
    /// Gets the next level after the specified level in hierarchy order.
    /// </summary>
    /// <param name="currentLevel">The current level.</param>
    /// <returns>The next level, or null if current level is the last.</returns>
    public int? GetNextLevel(int currentLevel)
    {
        var orderedLevels = GetOrderedLevels();
        var currentIndex = orderedLevels.ToList().IndexOf(currentLevel);
        
        if (currentIndex >= 0 && currentIndex < orderedLevels.Count - 1)
        {
            return orderedLevels[currentIndex + 1];
        }
        
        return null; // No next level
    }
    
    /// <summary>
    /// Gets the previous level before the specified level in hierarchy order.
    /// </summary>
    /// <param name="currentLevel">The current level.</param>
    /// <returns>The previous level, or null if current level is the first.</returns>
    public int? GetPreviousLevel(int currentLevel)
    {
        var orderedLevels = GetOrderedLevels();
        var currentIndex = orderedLevels.ToList().IndexOf(currentLevel);
        
        if (currentIndex > 0)
        {
            return orderedLevels[currentIndex - 1];
        }
        
        return null; // No previous level
    }
    
    /// <summary>
    /// Gets the total number of levels in this hierarchy.
    /// </summary>
    /// <returns>The total number of levels.</returns>
    public int GetLevelCount()
    {
        return Levels.Count;
    }
    
    /// <inheritdoc />
    public bool Equals(UserDefinedHierarchy? other)
    {
        return other is not null &&
               string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase);
    }
    
    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is UserDefinedHierarchy other && Equals(other);
    }
    
    /// <inheritdoc />
    public override int GetHashCode()
    {
        return StringComparer.OrdinalIgnoreCase.GetHashCode(Name);
    }
    
    /// <inheritdoc />
    public override string ToString()
    {
        var levelCount = Levels.Count;
        var description = Description is not null ? $" ({Description})" : "";
        return $"{Name}: {levelCount} levels{description}";
    }
    
    /// <summary>
    /// Equality operator for UserDefinedHierarchy.
    /// </summary>
    public static bool operator ==(UserDefinedHierarchy? left, UserDefinedHierarchy? right)
    {
        return left?.Equals(right) ?? right is null;
    }
    
    /// <summary>
    /// Inequality operator for UserDefinedHierarchy.
    /// </summary>
    public static bool operator !=(UserDefinedHierarchy? left, UserDefinedHierarchy? right)
    {
        return !(left == right);
    }
}