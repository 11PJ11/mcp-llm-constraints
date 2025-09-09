# New Test Project Integration Checklist

This checklist ensures that new test projects are properly integrated into the build and CI/CD pipeline, preventing the "Solution File Deception Anti-Pattern" where some projects are excluded from validation.

## Background

In September 2025, we discovered that two test projects (`ConstraintMcpServer.FeedbackTests` and `ConstraintMcpServer.StorageTests`) were created but not properly integrated into the development workflow, causing:

- **Test Coverage Gaps**: Only 338 tests running instead of expected 356
- **False Local Validation**: Quality gates were not validating all projects
- **Solution File Deception**: Projects existed but weren't included in solution builds

**Root Cause**: Process gap in development workflow - no systematic integration of new test projects.

## Checklist for New Test Projects

### ✅ 1. Project Creation
- [ ] Create test project using `dotnet new nunit` or appropriate template
- [ ] Follow naming convention: `{MainProject}.{Domain}Tests` (e.g., `ConstraintMcpServer.FeedbackTests`)
- [ ] Place in `tests/` directory
- [ ] Add appropriate NuGet package references (NUnit, FluentAssertions, etc.)

### ✅ 2. Solution File Integration
- [ ] **CRITICAL**: Add project to solution file using `dotnet sln add tests/{ProjectName}/{ProjectName}.csproj`
- [ ] Verify project appears in solution file by running `dotnet sln list`
- [ ] Test solution build: `dotnet build` should include the new project

### ✅ 3. Quality Gates Integration
**Note**: Since September 2025, quality gates use dynamic test discovery, so manual updates are not required. However, validate that discovery works:

- [ ] Run `find tests -name "*.csproj"` and verify new project is discovered
- [ ] Run `./scripts/quality-gates.sh` and verify:
  - New project is found during dynamic discovery
  - Project builds successfully
  - Tests are executed and counted correctly
  - No warnings about missing projects in solution file

### ✅ 4. CI/CD Pipeline Validation
- [ ] Run full quality gates locally: `./scripts/quality-gates.sh`
- [ ] Verify test count includes tests from new project
- [ ] Check solution file validation passes (no warnings about missing projects)
- [ ] Test in clean environment to ensure no dependency issues

### ✅ 5. Documentation Updates
- [ ] Update expected test count in quality gates if applicable
- [ ] Update project structure documentation if new patterns introduced
- [ ] Add any special build or test requirements to project README

### ✅ 6. Final Validation
- [ ] **Manual Test**: Delete `bin/` and `obj/` folders, run `dotnet build` - should succeed
- [ ] **Manual Test**: Run `dotnet test` on solution level - should include new project
- [ ] **Manual Test**: Run `./scripts/quality-gates.sh` - should discover and validate new project
- [ ] **Integration Test**: Create test branch and push to trigger CI - should pass

## Anti-Patterns to Avoid

### ❌ Solution File Deception Anti-Pattern
```bash
# DANGEROUS - hides compilation errors from disabled projects
dotnet test  # Only tests solution-enabled projects
```

**Prevention**: Always use dynamic discovery or explicit project paths:
```bash
# CORRECT - validates ALL projects explicitly
find tests -name "*.csproj" -exec dotnet test {} \;
```

### ❌ Hardcoded Project Lists
**Old approach** (brittle):
```bash
# Hardcoded list becomes stale when new projects added
dotnet test tests/Project1.Tests/
dotnet test tests/Project2.Tests/
# Missing: Project3.Tests, Project4.Tests...
```

**New approach** (resilient):
```bash
# Dynamic discovery automatically includes new projects
find tests -name "*.csproj" -exec dotnet test {} \;
```

### ❌ Manual Quality Gate Updates
**Old approach**: Manual editing of scripts when projects added  
**New approach**: Dynamic discovery handles new projects automatically

## Prevention Measures Implemented

### 1. Dynamic Test Discovery (September 2025)
- **scripts/quality-gates.sh** now uses `find tests -name "*.csproj"` for discovery
- Automatically detects new test projects without manual updates
- Prevents hardcoded project list staleness

### 2. Solution File Validation
- Quality gates now validate that all discovered test projects are included in solution file
- Warns about missing projects that could cause build inconsistencies
- Helps prevent Solution File Deception anti-pattern

### 3. Test Count Validation
- Quality gates track expected test count (currently 356)
- Alerts when test count changes, indicating new tests or missing projects
- Provides early detection of integration issues

### 4. Enhanced Error Messages
- Clear guidance when validation failures occur
- Specific suggestions for fixing common integration problems
- Links back to this checklist for systematic resolution

## Quick Reference Commands

```bash
# Add new test project to solution
dotnet sln add tests/{NewProject}/{NewProject}.csproj

# Verify solution includes all projects
dotnet sln list

# Test dynamic discovery
find tests -name "*.csproj"

# Run full quality gates
./scripts/quality-gates.sh

# Test specific project
dotnet test tests/{NewProject}/{NewProject}.csproj
```

## Troubleshooting

### Issue: "Test project found but not included in solution file"
**Solution**: 
```bash
dotnet sln add tests/{ProjectName}/{ProjectName}.csproj
```

### Issue: "Test count decreased"
**Possible Causes**:
- New project not included in solution file
- Tests marked as ignored or disabled
- Dynamic discovery not finding project files
- Build failures preventing test execution

**Investigation**:
```bash
# Check discovery
find tests -name "*.csproj"

# Check solution inclusion
dotnet sln list | grep -i test

# Manual test execution
dotnet test tests/{ProjectName}/{ProjectName}.csproj --verbosity normal
```

### Issue: Build failures in CI but not locally
**Common Cause**: Solution file includes projects locally but not in CI environment  
**Solution**: Ensure solution file is committed with all project inclusions

## Success Metrics

After following this checklist, you should see:
- ✅ New project discovered by `find tests -name "*.csproj"`
- ✅ Project included in `dotnet sln list` output
- ✅ No warnings in quality gates about missing solution projects
- ✅ Test count increased appropriately in quality gates output
- ✅ CI/CD pipeline includes new project in validation

## Contact

For questions about test project integration or issues with this checklist, consult the development team or update this document with new learnings.

---

**Document Version**: 1.0  
**Last Updated**: September 2025  
**Next Review**: When new test project integration patterns emerge