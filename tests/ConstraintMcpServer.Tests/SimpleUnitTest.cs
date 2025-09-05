using System;
using NUnit.Framework;
using FluentAssertions;
using ConstraintMcpServer.Domain.Distribution;

namespace ConstraintMcpServer.Tests;

/// <summary>
/// Simple unit test to verify project compilation and domain objects work correctly.
/// This is a stepping stone to get the project building before complex service tests.
/// </summary>
[TestFixture]
public class SimpleUnitTest
{
    [Test]
    public void InstallationResult_Success_ShouldCreateValidResult()
    {
        // Arrange
        var installationPath = "/usr/local/bin/constraint-server";
        var platform = PlatformType.Windows;
        var timeSeconds = 2.5;
        var configurationCreated = true;
        var pathConfigured = true;

        // Act
        var result = InstallationResult.Success(
            installationPath,
            platform,
            timeSeconds,
            configurationCreated,
            pathConfigured);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.InstallationPath.Should().Be(installationPath);
        result.Platform.Should().Be(platform);
        result.InstallationTimeSeconds.Should().Be(timeSeconds);
        result.ConfigurationCreated.Should().Be(configurationCreated);
        result.PathConfigured.Should().Be(pathConfigured);
    }

    [Test]
    public void InstallationResult_Failure_ShouldCreateFailedResult()
    {
        // Arrange
        var platform = PlatformType.Linux;
        var errorMessage = "Installation failed due to insufficient permissions";

        // Act
        var result = InstallationResult.Failure(platform, errorMessage);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Platform.Should().Be(platform);
        result.ErrorMessage.Should().Be(errorMessage);
    }

    [Test]
    public void InstallationOptions_ForPlatform_ShouldCreateValidOptions()
    {
        // Arrange
        var platform = PlatformType.MacOS;

        // Act
        var options = InstallationOptions.ForPlatform(platform);

        // Assert
        options.Should().NotBeNull();
        options.Platform.Should().Be(platform);
    }
}
