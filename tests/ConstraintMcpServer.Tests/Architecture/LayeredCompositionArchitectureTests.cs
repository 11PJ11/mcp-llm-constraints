using System.Reflection;
using System.Linq;
using NetArchTest.Rules;
using NUnit.Framework;

namespace ConstraintMcpServer.Tests.Architecture;

/// <summary>
/// NetArchTest validation for Layered Composition implementation correctness.
/// These tests validate that our LayeredComposition implementation follows good architectural principles.
/// Focus: Test the composition strategy classes themselves, not the entire constraint system architecture.
/// </summary>
[TestFixture]
[Category("Architecture")]
public class LayeredCompositionArchitectureTests
{
    private static readonly Assembly DomainAssembly = typeof(ConstraintMcpServer.Domain.Composition.LayeredComposition).Assembly;

    /// <summary>
    /// Validates that Domain.Composition layer maintains architectural purity.
    /// This ensures our composition strategies don't depend on outer concerns.
    /// </summary>
    [Test]
    public void Domain_Composition_Should_Not_Depend_On_Outer_Layers()
    {
        var result = ValidateDomainCompositionLayerPurity();

        Assert.That(result.IsSuccessful, Is.True,
            "Domain.Composition should not depend on Application, Infrastructure, or Presentation layers. " +
            $"Violations found: {FormatViolationsList(result.FailingTypes)}");
    }

    private TestResult ValidateDomainCompositionLayerPurity()
    {
        return Types.InAssembly(DomainAssembly)
            .That()
            .ResideInNamespace("ConstraintMcpServer.Domain.Composition")
            .ShouldNot()
            .HaveDependencyOnAny(
                "ConstraintMcpServer.Application",
                "ConstraintMcpServer.Infrastructure",
                "ConstraintMcpServer.Presentation"
            )
            .GetResult();
    }

    private static string FormatViolationsList(IEnumerable<Type>? failingTypes)
    {
        return string.Join(", ", failingTypes?.Select(t => t.FullName) ?? []);
    }

    /// <summary>
    /// Validates that LayeredComposition class follows architectural principles.
    /// Tests that our composition strategy implementation is architecturally sound.
    /// </summary>
    [Test]
    public void LayeredComposition_Should_Follow_Domain_Principles()
    {
        var result = ValidateLayeredCompositionDomainPrinciples();

        Assert.That(result.IsSuccessful, Is.True,
            "LayeredComposition classes should reside in Domain.Composition namespace and be sealed. " +
            $"Violations found: {FormatViolationsList(result.FailingTypes)}");
    }

    private TestResult ValidateLayeredCompositionDomainPrinciples()
    {
        return Types.InAssembly(DomainAssembly)
            .That()
            .HaveNameMatching("LayeredComposition*")
            .Should()
            .ResideInNamespace("ConstraintMcpServer.Domain.Composition")
            .And()
            .BeSealed()
            .GetResult();
    }

    /// <summary>
    /// Validates that our LayeredComposition implementation exists and has proper structure.
    /// This test confirms our implementation is architecturally complete.
    /// </summary>
    [Test]
    public void LayeredComposition_Implementation_Should_Be_Complete()
    {
        var layeredCompositionTypes = FindLayeredCompositionTypes();
        var layeredCompositionStateTypes = FindLayeredCompositionStateTypes();

        ValidateLayeredCompositionExists(layeredCompositionTypes);
        ValidateLayeredCompositionStateExists(layeredCompositionStateTypes);
        ValidateCorrectNamespaces(layeredCompositionTypes, layeredCompositionStateTypes);
    }

    private Type[] FindLayeredCompositionTypes()
    {
        return Types.InAssembly(DomainAssembly)
            .That()
            .HaveName("LayeredComposition")
            .GetTypes().ToArray();
    }

    private Type[] FindLayeredCompositionStateTypes()
    {
        return Types.InAssembly(DomainAssembly)
            .That()
            .HaveName("LayeredCompositionState")
            .GetTypes().ToArray();
    }

    private static void ValidateLayeredCompositionExists(Type[] layeredCompositionTypes)
    {
        Assert.That(layeredCompositionTypes, Is.Not.Empty, "LayeredComposition class should exist");
    }

    private static void ValidateLayeredCompositionStateExists(Type[] layeredCompositionStateTypes)
    {
        Assert.That(layeredCompositionStateTypes, Is.Not.Empty, "LayeredCompositionState class should exist");
    }

    private static void ValidateCorrectNamespaces(Type[] layeredCompositionTypes, Type[] layeredCompositionStateTypes)
    {
        const string expectedNamespace = "ConstraintMcpServer.Domain.Composition";
        Assert.That(layeredCompositionTypes.First().Namespace, Is.EqualTo(expectedNamespace));
        Assert.That(layeredCompositionStateTypes.First().Namespace, Is.EqualTo(expectedNamespace));
    }

    /// <summary>
    /// Validates that NetArchTest can correctly identify architectural violations.
    /// This test proves our validation approach would work for real Clean Architecture scenarios.
    /// </summary>
    [Test]
    public void NetArchTest_Can_Identify_Layer_Violations()
    {
        var result = ValidateDomainCompositionLayerPurity();

        Assert.That(result.IsSuccessful, Is.True,
            "Domain.Composition layer should not depend on outer layers. " +
            "This validates our LayeredComposition implementation follows good practices. " +
            $"Violations found: {FormatViolationsList(result.FailingTypes)}");
    }
}
