# SMS Viewer Development Constitution & Agent Guidelines

## Mandatory Agent Directives

All agents working on this SMS Viewer project **MUST** follow these non-negotiable principles. Violations of this constitution will result in code rejection and task reassignment.

---

## 1. SOLID Principles (Non-Negotiable)

Every code change must adhere strictly to SOLID:

- **S**ingle Responsibility: One class = one reason to change
- **O**pen/Closed: Open for extension, closed for modification
- **L**iskov Substitution: Derived types must be substitutable
- **I**nterface Segregation: Clients should depend on focused interfaces
- **D**ependency Inversion: Depend on abstractions, not concretions

**Agent Enforcement**: Before approving any PR/commit, validate that:
- Each class has a single, well-defined responsibility
- No class violates the DIP (no direct dependencies on concrete implementations)
- Interfaces are specific to client needs (no "fat" interfaces)

---

## 2. Clean Code Standards

All code must be clean and maintainable:

### Naming Conventions
- **Classes/Methods**: PascalCase (e.g., `XmlSmsParser`, `ParseMessages()`)
- **Variables/Parameters**: camelCase (e.g., `threadId`, `messageContent`)
- **Constants**: UPPER_SNAKE_CASE (e.g., `MAX_FILE_SIZE`)
- **Private fields**: `_camelCase` (e.g., `_repository`, `_logger`)

### Code Limits
- **Maximum method length**: 30 lines
- **Maximum class length**: 300 lines (refactor into smaller, focused classes)
- **Maximum nesting depth**: 3 levels
- **Maximum parameters per method**: 3 (use objects/DTOs for more)

### Comments & Documentation
- Only comment code that requires explanation; avoid obvious comments
- Use XML documentation for public APIs
- Commit messages must reference OpenSpec task IDs (e.g., `[1.1] Implement XML parser`)

### Error Handling
- Use specific exception types, never catch and swallow silently
- Log errors with full context (no sensitive data)
- Return meaningful error messages to users

---

## 3. Atomic TDD (Test-First Development)

**No production code without tests.** This is non-negotiable.

### Test Requirements
- Write tests **BEFORE** implementation (red → green → refactor)
- **One test = one behavior/scenario**
- Each test must be independent; run in any order
- Test naming: Clear, BDD-style (e.g., `When_LargeFileLoaded_Should_ParseCorrectly`)
- **Minimum code coverage**: 80% (validated in CI)

### Test Isolation
- No shared state between tests
- No database/file dependencies (use mocks)
- Each test can run independently
- Use dependency injection for testability

### Example Test Structure
```csharp
[Fact]
public void When_XmlFileIsValid_Should_ParseAllMessages()
{
    // Arrange
    var xmlContent = @"<messages>...</messages>";
    var parser = new XmlSmsParser();
    
    // Act
    var result = parser.Parse(xmlContent);
    
    // Assert
    Assert.NotEmpty(result);
    Assert.All(result, msg => Assert.NotNull(msg.Content));
}
```

---

## 4. Git & Commit Discipline

- **Commits must be atomic**: One logical change = one commit
- **Commit messages**: Format as `[TASK-ID] Clear description` (e.g., `[2.1] Create SmsThread domain model`)
- **Branch naming**: Follow `feature/task-id-description` pattern
- **No merging without review**: All PRs require peer review before merge
- **PR description**: Link to OpenSpec task/proposal; describe changes clearly

---

## 5. Architecture & Design

### Repository Pattern
- Data access must be abstracted behind `IRepository<T>` interface
- No direct database/file access from UI or business logic
- Dependency injection for all repositories

### MVVM for UI
- ViewModels must not contain UI logic
- Binding-friendly properties using ReactiveUI (if available) or property change notifications
- No code-behind beyond trivial UI setup

### Streaming for Large Files
- Large XML files must be parsed using streaming (XmlReader)
- No loading entire files into memory
- Progress feedback for user during parsing

---

## 6. OpenSpec Workflow Compliance

Every task must follow the OpenSpec flow:

1. **New Change**: `/opsx:new task-name` creates a new change directory
2. **Proposal**: Define the problem, approach, and success criteria
3. **Specs**: Write requirements and scenarios
4. **Design**: Document technical approach
5. **Tasks**: Create implementation checklist
6. **Implementation**: Code per tasks with tests
7. **Archive**: Move to archive when complete

**Agent Responsibility**: Ensure all files are created before implementing.

---

## 7. Code Review Checklist for Agents

Before approving any code, validate:

- [ ] All tests pass and coverage ≥ 80%
- [ ] No SOLID principle violations
- [ ] Method length ≤ 30 lines
- [ ] Class length ≤ 300 lines
- [ ] Naming conventions consistent
- [ ] No hardcoded values (use config)
- [ ] Error handling is complete
- [ ] No sensitive data in logs
- [ ] XML documentation present on public methods
- [ ] Commit messages reference task IDs
- [ ] No style/formatting inconsistencies

---

## 8. Definition of Done (DoD)

A task is **NOT** done until:

- ✅ All unit tests written and passing (TDD flow followed)
- ✅ Code coverage ≥ 80%
- ✅ All SOLID principles satisfied
- ✅ Clean code standards met
- ✅ Code review approved
- ✅ No build warnings or static analysis violations
- ✅ OpenSpec task marked complete
- ✅ Commit message references task ID
- ✅ Documentation updated if applicable

---

## 9. Agent Escalation Policy

If an agent encounters any of the following, **STOP and escalate to the user**:

- Ambiguity in requirements
- Design decision affecting architecture
- Scope creep or unclear boundaries
- Conflicts between constitution requirements
- Technical blockers or feasibility questions

**Do NOT proceed without clarity.**

---

## 10. Technology Stack (Fixed)

- **Language**: C# (.NET 8+)
- **UI Framework**: Avalonia
- **Architecture**: MVVM
- **Testing**: xUnit + Moq
- **XML**: System.Xml (streaming with XmlReader)
- **CI/CD**: GitHub Actions

**No deviations without explicit user approval.**

---

## Enforcement

- Every PR must include a `[✓ CONSTITUTION CHECK]` comment verifying compliance
- Agents must reference this document when requesting changes
- Violations result in PR rejection and resubmission
- Repeated violations may result in agent reassignment

---

**Version**: 1.0  
**Last Updated**: 2026-02-03  
**Status**: ACTIVE - All agents must follow
