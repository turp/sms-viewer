## Context

The application needs to load and display SMS/MMS messages from XML backup files produced by common Android backup apps. These files can be several hundred megabytes in size, requiring memory-efficient parsing. The project uses Avalonia for the UI and follows strict MVVM and SOLID principles.

## Goals / Non-Goals

**Goals:**
- implement streaming XML parsing using `XmlReader` to handle large files without loading the entire DOM into memory.
- Provide a responsive UI that doesn't freeze during file loading.
- Separate data access for XML files behind a repository interface.
- Support both SMS and MMS message types as defined in the provided schema.

**Non-Goals:**
- Modifying or writing to the XML backup files.
- Implementing complex search or filtering in this initial phase.
- Supporting encrypted backup formats.

## Decisions

### 1. Repository Pattern for Data Access
- **Decision**: Abstract XML reading behind an `ISmsRepository` interface.
- **Rationale**: Decouples the UI and business logic from the specific storage format (XML), making it easier to test and potentially support other formats (JSON, Database) in the future.

### 2. Streaming Parser (XmlReader)
- **Decision**: Use `System.Xml.XmlReader` for one-way, forward-only streaming.
- **Rationale**: Essential for performance and memory management with large backup files. Avoids `XmlDocument` or `XDocument` which would load the entire tree into RAM.

### 3. Unified Message Model
- **Decision**: Create a base `IMessage` interface or abstract class, with specific `SmsMessage` and `MmsMessage` records.
- **Rationale**: Allows the UI to handle a collection of mixed message types while preserving specific attributes for each (e.g., MMS parts).

### 4. Async Loading in ViewModel
- **Decision**: Use asynchronous methods (`IAsyncEnumerable` or `Task`) to load messages into the ViewModel.
- **Rationale**: Keeps the Avalonia UI thread responsive while the parser processes the file in the background.

## Risks / Trade-offs

- **[Risk]** Large XML files may cause a slow initial load of the message list.
  - **Mitigation**: Implement a progressive loading approach or virtualization in the Avalonia `ListBox`.
- **[Risk]** Memory bloat if the entire list of messages is held in the ViewModel.
  - **Mitigation**: Investigate UI virtualization or only loading metadata initially if memory becomes a bottleneck.
- **[Risk]** Inconsistent XML formats from different backup tools.
  - **Mitigation**: Base the parser strictly on the provided XSD and log/skip invalid nodes.
