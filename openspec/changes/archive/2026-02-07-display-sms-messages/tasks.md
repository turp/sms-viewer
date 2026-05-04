## 1. Domain Models & Abstractions

- [x] 1.1 Create `IMessage` interface and `SmsMessage`, `MmsMessage` records in `Models/`.
- [x] 1.2 Define `ISmsRepository` interface with `GetMessagesAsync(string filePath)` returning `IAsyncEnumerable<IMessage>`.

## 2. XML Parsing Implementation (TDD)

- [x] 2.1 Create unit tests for `XmlSmsRepository` using small XML samples.
- [x] 2.2 Implement `XmlSmsRepository` using `XmlReader` for streaming SMS elements.
- [x] 2.3 Extend `XmlSmsRepository` to handle MMS elements and their nested `<part>` tags.
- [x] 2.4 Implement basic schema-aware validation during parsing.

## 3. File Management & Services

- [x] 3.1 Implement a service to wrap Avalonia's `StorageProvider` for selecting XML files.
- [x] 3.2 Add error handling for file access issues (missing files, permissions).

## 4. ViewModel Logic

- [x] 4.1 Update `MainWindowViewModel` to include an `ObservableCollection<IMessage>` for displayed messages.
- [x] 4.2 Add `OpenXmlFileCommand` to `MainWindowViewModel` that triggers the file service and repository.
- [x] 4.3 Implement progressive loading in the ViewModel to keep the UI fluid.

## 5. UI Implementation

- [x] 5.1 Update `MainWindow.axaml` with a Toolbar containing an "Open File" button.
- [x] 5.2 Implement a `ListBox` in `MainWindow.axaml` with `VirtualizingStackPanel`.
- [x] 5.3 Create DataTemplates for different message types (SMS vs MMS) to display content, date, and contact.

## 6. Verification

- [x] 6.1 Run all unit tests and ensure 80% coverage.
- [x] 6.2 Manual verification with `xml/sms-sample.xml`.
- [x] 6.3 Final [✓ CONSTITUTION CHECK] against `AGENTS.md`.
