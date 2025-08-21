# Configuration Directory

This directory contains YAML configuration files for the Constraint Enforcement MCP Server.

## Files

### `constraints.yaml`
Sample constraint pack demonstrating the configuration format. This includes:
- **TDD constraints**: Test-first development reminders
- **Architecture constraints**: Hexagonal architecture boundaries  
- **Quality constraints**: YAGNI and clean code principles
- **Scheduling configuration**: Injection cadence and anchors

## Implementation Status

**Current (Step 1)**: Sample configuration provided for reference
**Step 3**: YAML loading and validation will be implemented
**Step 4**: Scheduling configuration will become active
**Step 5**: Constraint selection and injection will use these definitions

## Usage

When the server is fully implemented (Step 3+), place your constraint packs in this directory and reference them in your MCP server configuration.

Example:
```bash
dotnet run --project src/ConstraintMcpServer -- --config ./config/constraints.yaml
```