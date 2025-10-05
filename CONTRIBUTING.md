# Contributing to Schulkueche Monatsabrechnung

Thank you for your interest in contributing to Schulkueche Monatsabrechnung! This document provides guidelines and information for contributors.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Setup](#development-setup)
- [How to Contribute](#how-to-contribute)
- [Development Guidelines](#development-guidelines)
- [Commit Guidelines](#commit-guidelines)
- [Pull Request Process](#pull-request-process)
- [Reporting Issues](#reporting-issues)
- [License](#license)

## Code of Conduct

This project and everyone participating in it is governed by our [Code of Conduct](CODE_OF_CONDUCT.md). By participating, you are expected to uphold this code. Please report unacceptable behavior to the project maintainers.

## Getting Started

### Prerequisites

- [.NET SDK 9.0](https://dotnet.microsoft.com/download/dotnet/9.0) or later
- [Git](https://git-scm.com/)
- IDE of choice (Visual Studio 2024, VS Code, or JetBrains Rider recommended)
- Basic knowledge of C#, Avalonia UI, and Entity Framework Core

### Fork and Clone

1. Fork the repository on GitHub
2. Clone your fork locally:
   ```bash
   git clone https://github.com/YOUR-USERNAME/SchulkuecheMonatsabrechnung.git
   cd SchulkuecheMonatsabrechnung
   ```

## Development Setup

### 1. Install Dependencies

```bash
# Restore NuGet packages
dotnet restore

# Install Entity Framework CLI tools (if not already installed)
dotnet tool install --global dotnet-ef
```

### 2. Build the Project

```bash
# Build the entire solution
dotnet build

# Run the application
dotnet run --project src/Schulkueche.App
```

### 3. Database Setup

The application uses SQLite and will automatically create the database on first run. The database file is located at `%LocalAppData%/Schulkueche/kitchen.db`.

For development, you might want to work with a fresh database:

```bash
# Apply migrations (creates database if it doesn't exist)
dotnet ef database update \
  -p src/Schulkueche.Data/Schulkueche.Data.csproj \
  -s src/Schulkueche.App/Schulkueche.App.csproj
```

## How to Contribute

### Areas Where We Welcome Contributions

- **Bug fixes**: Help us fix issues reported in GitHub Issues
- **Feature implementations**: Work on features from our roadmap
- **Documentation improvements**: Enhance README, code comments, or help documentation
- **Localization**: Add support for additional languages
- **Performance improvements**: Optimize database queries, UI rendering, etc.
- **Testing**: Add unit tests, integration tests, or UI tests
- **Accessibility improvements**: Make the application more accessible

### Types of Contributions

1. **Bug Reports** - Report bugs using our issue template
2. **Feature Requests** - Suggest new features or improvements
3. **Code Contributions** - Submit pull requests with bug fixes or new features
4. **Documentation** - Improve project documentation
5. **Translation** - Help translate the application to other languages

## Development Guidelines

### Project Structure

```
src/
├── Schulkueche.App/        # Avalonia UI Application (German UI)
│   ├── ViewModels/         # MVVM ViewModels
│   ├── Views/             # XAML Views
│   └── Infrastructure/    # DI Container Setup
├── Schulkueche.Core/      # Domain Models (English)
└── Schulkueche.Data/      # Data Access Layer (English)
    ├── Repositories/      # Repository Pattern
    └── Migrations/        # EF Core Migrations
```

### Coding Standards

#### Language Usage
- **German**: UI labels, error messages, ViewModel properties visible in XAML bindings
- **English**: Domain models, repository interfaces, service classes, code comments
- **Comments**: English for technical explanations, German for business logic

#### C# Guidelines
- Follow [Microsoft C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use modern C# features (pattern matching, nullable reference types, etc.)
- Prefer `ConfigureAwait(false)` in library code
- Use `CancellationToken` parameters for async methods

#### MVVM Patterns
- Use `CommunityToolkit.Mvvm` source generators
- Properties: `[ObservableProperty]`
- Commands: `[RelayCommand]`
- Async commands: suffix with `Async`
- German error messages via bindable `Status` properties

#### Database Guidelines
- All database access through repository interfaces
- Use bulk operations to prevent N+1 queries
- Always use cancellation tokens for async operations
- Include comprehensive error handling

### UI/UX Guidelines

#### Avalonia XAML
- Use `DynamicResource` for colors and themes
- Maintain responsive grid-based layouts
- Use 8px spacing grid for consistency
- German text in UI elements

#### Theme Support
- Support both Light and Dark themes
- Use system accent colors where appropriate
- Custom theme resources pattern:
  ```xml
  <ResourceDictionary x:Key="Light">
    <SolidColorBrush x:Key="AppSurfaceGray">#F0F0F0</SolidColorBrush>
  </ResourceDictionary>
  <ResourceDictionary x:Key="Dark">
    <SolidColorBrush x:Key="AppSurfaceGray">#121111</SolidColorBrush>
  </ResourceDictionary>
  ```

### Testing Guidelines

While we don't currently have extensive tests, we welcome contributions that add:

- Unit tests for business logic
- Integration tests for data access
- UI automation tests for critical workflows

Use the following testing frameworks:
- **Unit Tests**: xUnit + FluentAssertions
- **Mocking**: NSubstitute or Moq
- **UI Tests**: Avalonia.Testing (when available)

## Commit Guidelines

We follow [Conventional Commits](https://www.conventionalcommits.org/) specification:

### Commit Message Format

```
<type>: <description>

[optional body]

[optional footer(s)]
```

### Types

- `feat`: A new feature
- `fix`: A bug fix
- `docs`: Documentation changes
- `style`: UI/styling changes (not code style)
- `refactor`: Code changes that neither fix bugs nor add features
- `perf`: Performance improvements
- `test`: Adding or updating tests
- `build`: Build system changes
- `ci`: CI/CD changes

### Examples

```bash
feat: Add user authentication system with email verification

- Implement secure login/registration workflow
- Add password reset functionality with email codes
- Include session management and user state persistence

fix: Resolve LoginWindow layout issues in dark mode

The LoginWindow had text overflow issues in dark mode due to
insufficient contrast and fixed sizing. This change implements
dynamic theme resources for better visibility.

docs: Update README with comprehensive installation guide
```

### Important Notes

- **No emojis** in commit messages (per project guidelines)
- Use imperative mood ("Add feature" not "Added feature")
- First line should be 50 characters or less
- Body should explain what and why, not how

## Pull Request Process

### Before Submitting

1. **Test your changes** thoroughly
2. **Update documentation** if needed
3. **Follow code style** guidelines
4. **Write meaningful commit messages**
5. **Ensure builds pass** locally

### Pull Request Template

When creating a pull request, please include:

- **Description**: What does this PR do and why?
- **Type of Change**: Bug fix, feature, documentation, etc.
- **Testing**: How was this tested?
- **Breaking Changes**: List any breaking changes
- **Screenshots**: For UI changes, include before/after screenshots

### Review Process

1. **Automated Checks**: Ensure all GitHub Actions pass
2. **Code Review**: A maintainer will review your code
3. **Testing**: Changes will be tested in various scenarios
4. **Feedback**: Address any requested changes
5. **Merge**: Once approved, the PR will be merged

### After Merge

- Your changes will be included in the next release
- We'll update the changelog with your contribution
- Consider watching the repository for future updates

## Reporting Issues

### Bug Reports

Use our bug report template and include:

- **Environment**: OS, .NET version, application version
- **Steps to reproduce**: Detailed step-by-step instructions
- **Expected behavior**: What should happen
- **Actual behavior**: What actually happens
- **Screenshots**: If applicable
- **Additional context**: Any relevant information

### Feature Requests

For feature requests, please include:

- **Problem statement**: What problem does this solve?
- **Proposed solution**: Your preferred solution
- **Alternatives considered**: Other approaches you've considered
- **Use cases**: How would this be used?

### Priority Levels

- **Critical**: Security vulnerabilities, data loss issues
- **High**: Major functionality broken, significant user impact
- **Medium**: Minor bugs, feature requests
- **Low**: Documentation improvements, nice-to-have features

## Development Resources

### Useful Links

- [Avalonia UI Documentation](https://docs.avaloniaui.net/)
- [Entity Framework Core Documentation](https://docs.microsoft.com/ef/core/)
- [CommunityToolkit.Mvvm Documentation](https://docs.microsoft.com/windows/communitytoolkit/mvvm/)
- [QuestPDF Documentation](https://www.questpdf.com/documentation/)

### Project-Specific Documentation

- [README.md](README.md) - Project overview and setup
- [WARP.md](WARP.md) - Warp Terminal development guidelines
- [LICENSE](LICENSE) - GNU Affero General Public License v3

### Community

- **GitHub Discussions**: For questions and general discussion
- **Issues**: For bug reports and feature requests
- **Pull Requests**: For code contributions

## Recognition

Contributors are recognized in several ways:

- **Commit attribution**: Your commits will be attributed to your GitHub profile
- **Release notes**: Significant contributions mentioned in release notes
- **Contributors section**: Added to project contributors (when implemented)

## Questions?

If you have questions that aren't covered by this guide:

1. Check existing [GitHub Issues](https://github.com/Zobiii/SchulkuecheMonatsabrechnung/issues)
2. Create a new issue with the "question" label
3. Join our community discussions

## License

By contributing to Schulkueche Monatsabrechnung, you agree that your contributions will be licensed under the [GNU Affero General Public License v3](LICENSE).

---

Thank you for contributing to Schulkueche Monatsabrechnung! Your contributions help make this project better for the community kitchen management needs.