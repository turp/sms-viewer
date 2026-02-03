# SMS Viewer

A cross-platform SMS message reader application built with C# and Avalonia. Parse large XML files containing SMS threads with intuitive viewing, searching, filtering, and export capabilities.

## Features

- **Cross-Platform**: Runs on Windows and Linux
- **Large File Support**: Stream-based parsing for 100MB+ XML files
- **Search & Filter**: Find messages by text, date range, or contact
- **Export**: Save filtered messages as CSV
- **Statistics**: View thread summaries and message counts

## Development

This project uses **OpenSpec** for specification-driven development. See `AGENTS.md` for the development constitution that all contributors must follow.

### Quick Start

```bash
cd src
dotnet restore
dotnet build
dotnet run
```

### Running Tests

```bash
cd tests
dotnet test --verbosity normal
```

## Constitution

All development must adhere to the constitution outlined in `AGENTS.md`:
- **SOLID Principles**: Mandatory adherence
- **Clean Code**: Consistent naming, method/class size limits
- **Atomic TDD**: Write tests first, minimum 80% coverage
- **Git Discipline**: Atomic commits with task IDs

## Architecture

- **Pattern**: MVVM with Dependency Injection
- **UI**: Avalonia 11.3+
- **Testing**: xUnit with Moq
- **XML Parsing**: System.Xml with streaming support

## Project Structure

```
sms-viewer/
├── src/                    # Main application
│   ├── ViewModels/         # MVVM ViewModels
│   ├── Views/              # Avalonia UI
│   └── Models/             # Domain models
├── tests/                  # Unit tests (xUnit)
├── openspec/               # OpenSpec specifications
│   ├── changes/            # Active feature branches
│   └── archive/            # Completed features
├── AGENTS.md               # Development constitution
└── README.md               # This file
```

## License

MIT
