using System;
using NUnit.Framework;
using ConstraintMcpServer.Domain.Distribution;

namespace ConstraintMcpServer.Tests;

/// <summary>
/// Domain behavior tests that validate professional installation business rules.
/// These tests ensure domain objects deliver expected business outcomes and user value.
/// Focuses on business scenarios rather than technical implementation details.
/// </summary>
[TestFixture]
public class ProfessionalInstallationDomainBehaviorTests
{
    [Test]
    public void When_InstallationSucceeds_Should_ProvideSystemAccessibilityDetails()
    {
        // Business Scenario: Professional user successfully installs constraint system
        // Expected Outcome: User can immediately access and verify their working system
        
        // Arrange - Professional installation context
        var installationPath = "/usr/local/bin/constraint-server";
        var platform = PlatformType.Windows;
        var timeSeconds = 2.5; // Fast professional installation
        var configurationCreated = true;
        var pathConfigured = true;

        // Act - System reports successful installation
        var result = InstallationResult.Success(
            installationPath,
            platform,
            timeSeconds,
            configurationCreated,
            pathConfigured);

        // Assert - User receives complete system accessibility information
        Assert.That(result, Is.Not.Null, "user must receive installation confirmation");
        Assert.That(result.IsSuccess, Is.True, "user must know installation succeeded");
        Assert.That(result.InstallationPath, Is.EqualTo(installationPath), "user must know where system is installed for verification");
        Assert.That(result.Platform, Is.EqualTo(platform), "user must confirm correct platform installation");
        Assert.That(result.InstallationTimeSeconds, Is.EqualTo(timeSeconds), "user must see installation performance metrics");
        Assert.That(result.ConfigurationCreated, Is.EqualTo(configurationCreated), "user must know if system is ready to use");
        Assert.That(result.PathConfigured, Is.EqualTo(pathConfigured), "user must know if command line access is available");
    }

    [Test]
    public void When_InstallationFails_Should_ProvideClearErrorGuidance()
    {
        // Business Scenario: Professional user encounters installation failure
        // Expected Outcome: User receives actionable error information to resolve the issue
        
        // Arrange - Installation failure context
        var platform = PlatformType.Linux;
        var errorMessage = "Installation failed due to insufficient permissions";

        // Act - System reports installation failure
        var result = InstallationResult.Failure(platform, errorMessage);

        // Assert - User receives clear guidance for resolution
        Assert.That(result, Is.Not.Null, "user must receive failure notification");
        Assert.That(result.IsSuccess, Is.False, "user must clearly understand installation failed");
        Assert.That(result.Platform, Is.EqualTo(platform), "user must know which platform had the failure");
        Assert.That(result.ErrorMessage, Is.EqualTo(errorMessage), "user must receive actionable error guidance for resolution");
    }

    [Test]
    public void When_SelectingTargetPlatform_Should_ConfigureAppropriateInstallationStrategy()
    {
        // Business Scenario: Professional user selects their target platform for installation
        // Expected Outcome: System configures platform-specific installation approach
        
        // Arrange - User platform selection
        var platform = PlatformType.MacOS;

        // Act - System generates platform-appropriate installation strategy
        var options = InstallationOptions.ForPlatform(platform);

        // Assert - User receives platform-optimized installation configuration
        Assert.That(options, Is.Not.Null, "user must receive installation configuration");
        Assert.That(options.Platform, Is.EqualTo(platform), "system must configure for user's selected platform");
    }
}
